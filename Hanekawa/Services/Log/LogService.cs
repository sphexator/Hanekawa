using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Data.Constants;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Services.Log
{
    public class LogService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ILogger _commandsLogger;
        private readonly ILogger _discordLogger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ModerationService _moderationService;
        private readonly MuteService _muteService;
        private readonly WarnService _warnService;

        public LogService(DiscordSocketClient client, CommandService commands, ILoggerFactory loggerFactory,
            ModerationService moderationService, MuteService muteService, WarnService warnService)
        {
            _client = client;
            _commands = commands;
            _moderationService = moderationService;
            _muteService = muteService;
            _warnService = warnService;

            _loggerFactory = ConfigureLogging(loggerFactory);
            _discordLogger = _loggerFactory.CreateLogger("client");
            _commandsLogger = _loggerFactory.CreateLogger("commands");

            _client.Log += LogDiscord;
            _commands.Log += LogCommand;

            _client.UserBanned += Banned;
            _client.UserUnbanned += Unbanned;
            _client.UserJoined += UserJoined;
            _client.UserLeft += UserLeft;
            _client.MessageDeleted += MessageDeleted;
            _client.MessageUpdated += MessageUpdated;

            _client.GuildMemberUpdated += GuildUserUpdated;
            _client.UserUpdated += UserUpdated;

            _muteService.UserMuted += UserMuted;
            _muteService.UserTimedMuted += UserTimedMute;
            _muteService.UserUnmuted += UserUnmuted;

            _moderationService.AutoModPermLog += AutoModPermLog;
            _moderationService.AutoModTimedLog += AutoModTimedLog;

            _warnService.UserWarned += UserWarnLog;
            _warnService.UserMuted += UserMuteWarnLog;
        }

        private static Task UserUpdated(SocketUser oldUsr, SocketUser newUsr)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    if (!(newUsr is SocketGuildUser gusr)) return;
                    var cfg = await db.GetOrCreateGuildConfig(gusr.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var ch = gusr.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder {Color = Color.Purple};
                    if (oldUsr.Username != newUsr.Username)
                    {
                        embed.WithTitle("Username Change")
                            .WithDescription($"{oldUsr.Username}#{oldUsr.Discriminator} || {oldUsr.Id}")
                            .AddField(x => x.WithName("Old Name").WithValue($"{oldUsr.Username}").WithIsInline(true))
                            .AddField(x => x.WithName("New Name").WithValue($"{newUsr.Username}").WithIsInline(true));
                    }
                    else if (oldUsr.AvatarId != newUsr.AvatarId)
                    {
                        embed.WithTitle("Avatar Change")
                            .WithDescription($"{oldUsr.Username}#{oldUsr.Discriminator} | {oldUsr.Id}");

                        if (Uri.IsWellFormedUriString(oldUsr.GetAvatarUrl(ImageFormat.Auto, 1024), UriKind.Absolute))
                            embed.WithThumbnailUrl(oldUsr.GetAvatarUrl(ImageFormat.Auto, 1024));
                        if (Uri.IsWellFormedUriString(newUsr.GetAvatarUrl(ImageFormat.Auto, 1024), UriKind.Absolute))
                            embed.WithImageUrl(newUsr.GetAvatarUrl(ImageFormat.Auto, 1024));
                    }
                    else return;

                    await ch.SendMessageAsync(null, false, embed.Build());
                }
            });
            return Task.CompletedTask;
        }

        private static Task GuildUserUpdated(SocketGuildUser oldUsr, SocketGuildUser newUsr)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(newUsr.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var ch = newUsr.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (ch == null) return;
                    var footer = new EmbedFooterBuilder
                    {
                        IconUrl = newUsr.GetAvatar(),
                        Text = ""
                    };
                    var embed = new EmbedBuilder
                    {
                        Title = $"{oldUsr.Username}#{oldUsr.Discriminator} | {oldUsr.Id}",
                        Footer = footer
                    };
                    if (oldUsr.Nickname != newUsr.Nickname)
                    {
                        embed.WithAuthor(x => x.WithName("Nick Change"))
                            .AddField(x => x.WithName("Old Nick").WithValue($"{oldUsr.Nickname}#{oldUsr.Discriminator}"))
                            .AddField(x => x.WithName("New Nick").WithValue($"{newUsr.Nickname}#{newUsr.Discriminator}"));
                        await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                    }
                    else if (!oldUsr.Roles.SequenceEqual(newUsr.Roles))
                    {
                        if (oldUsr.Roles.Count < newUsr.Roles.Count)
                        {
                            var roleDiffer = newUsr.Roles.Where(x => !oldUsr.Roles.Contains(x)).Select(x => x.Name);
                            embed.WithAuthor(x => x.WithName("User role added"))
                                .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                        }
                        else if (oldUsr.Roles.Count > newUsr.Roles.Count)
                        {
                            var roleDiffer = oldUsr.Roles.Where(x => !newUsr.Roles.Contains(x)).Select(x => x.Name);
                            embed.WithAuthor(x => x.WithName("User role removed"))
                                .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                        }

                        await ch.SendMessageAsync(null, false, embed.Build());
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserMuteWarnLog(SocketGuildUser user, string staff, string reason, TimeSpan duration)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    if (!cfg.LogWarn.HasValue) return;
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Purple,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Author = new EmbedAuthorBuilder { Name = "User Mute Warned" }
                    };
                    embed.AddField("User", $"{user.Mention}", true);
                    embed.AddField("Staff", staff, true);
                    embed.AddField("Reason", reason.Truncate(700), true);
                    embed.AddField("Duration", $"{duration.Humanize()} ({duration})", true);
                    embed.WithFooter($"Username: {user.Username}#{user.Discriminator} ({user.Id})");
                    var ch = user.Guild.GetTextChannel(cfg.LogWarn.Value);
                    await ch.SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserWarnLog(SocketGuildUser user, string staff, string reason)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    if (!cfg.LogWarn.HasValue) return;
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Purple,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Author = new EmbedAuthorBuilder { Name = "User Warned" }
                    };
                    embed.AddField("User", $"{user.Mention}", true);
                    embed.AddField("Staff", staff, true);
                    embed.AddField("Reason", reason.Truncate(700), true);
                    embed.WithFooter($"Username: {user.Username}#{user.Discriminator} ({user.Id})");
                    var ch = user.Guild.GetTextChannel(cfg.LogWarn.Value);
                    await ch.SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task AutoModTimedLog(SocketGuildUser user, ModerationService.AutoModActionType type,
            TimeSpan timer, string msg)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Mute);
                    var author =
                        new EmbedAuthorBuilder { Name = $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.Discriminator}" };
                    var footer =
                        new EmbedFooterBuilder { Text = $"User ID: {user.Id}" };
                    var userField =
                        new EmbedFieldBuilder { Name = "User", Value = user.Mention, IsInline = false };
                    var reasonField =
                        new EmbedFieldBuilder { Name = "Reason", Value = type, IsInline = false };
                    var modField =
                        new EmbedFieldBuilder { Name = "Moderator", Value = "Auto-Moderator", IsInline = false };
                    var duration =
                        new EmbedFieldBuilder { Name = "Duration", Value = timer.Humanize(), IsInline = false };
                    var message =
                        new EmbedFieldBuilder { Name = "Message", Value = msg.Truncate(999), IsInline = false };
                    var result = new List<EmbedFieldBuilder> { userField, modField, reasonField, duration, message };

                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = result
                    };

                    await user.Guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                    caseId.ModId = 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task AutoModPermLog(SocketGuildUser user, ModerationService.AutoModActionType type, string msg)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Mute);
                    var author = new EmbedAuthorBuilder
                    {
                        Name = $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.Discriminator}"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User ID: {user.Id}"
                    };

                    var userField = new EmbedFieldBuilder
                    {
                        Name = "User",
                        Value = user.Mention,
                        IsInline = false
                    };
                    var reasonField = new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = type,
                        IsInline = false
                    };
                    var modField = new EmbedFieldBuilder
                    {
                        Name = "Moderator",
                        Value = "Auto-Moderator",
                        IsInline = false
                    };
                    var message = new EmbedFieldBuilder
                    {
                        Name = "Message",
                        Value = msg.Truncate(999),
                        IsInline = false
                    };
                    var result = new List<EmbedFieldBuilder> { userField, modField, reasonField, message };

                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = result
                    };

                    await user.Guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                    caseId.ModId = 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserUnmuted(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;
                    var author = new EmbedAuthorBuilder
                    {
                        Name = $"{user.Username}#{user.Discriminator} | {ActionType.Ungagged}"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User ID: {user.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Description = $"{user.Mention}",
                        Footer = footer,
                        Color = new Color(Color.Green.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow)
                    };
                    await user.Guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserTimedMute(SocketGuildUser user, SocketGuildUser staff, TimeSpan timer)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Mute);
                    var author = new EmbedAuthorBuilder
                    {
                        Name = $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.Discriminator}"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User ID: {user.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = ModLogFieldBuilders(user, null, timer)
                    };
                    var msg = await user.Guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                    caseId.MessageId = msg.Id;
                    caseId.ModId = staff.Id;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserMuted(SocketGuildUser user, SocketGuildUser staff)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Mute);
                    var author = new EmbedAuthorBuilder
                    {
                        Name = $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.Discriminator}"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User ID: {user.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = ModLogFieldBuilders(user)
                    };
                    var msg = await user.Guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                    caseId.MessageId = msg.Id;
                    caseId.ModId = staff.Id;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserJoined(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogJoin.HasValue) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📥 {user.Mention} has joined (*{user.Id}*)\n" +
                                      $"Account Created: {user.CreatedAt.Humanize()}",

                        Color = new Color(Color.Green.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator}" }
                    };

                    await user.Guild.GetTextChannel(cfg.LogJoin.Value)
                        .SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserLeft(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogJoin.HasValue) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📤 {user.Mention} has left (*{user.Id}*)",
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Color = new Color(Color.Red.RawValue),
                        Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator}" }
                    };

                    await user.Guild.GetTextChannel(cfg.LogJoin.Value)
                        .SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private static Task Banned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    try
                    {
                        var cfg = await db.GetOrCreateGuildConfig(guild).ConfigureAwait(false);
                        if (!cfg.LogBan.HasValue) return;

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Ban);
                        var author = new EmbedAuthorBuilder
                        {
                            Name = $"Case ID: {caseId.Id} - {ActionType.Bent} | {user.Username}#{user.Discriminator}"
                        };
                        var footer = new EmbedFooterBuilder
                        {
                            Text = $"User ID: {user.Id}"
                        };
                        var embed = new EmbedBuilder
                        {
                            Author = author,
                            Footer = footer,
                            Color = Color.Red,
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = ModLogFieldBuilders(user)
                        };
                        var msg = await guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                        caseId.MessageId = msg.Id;
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static Task Unbanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    try
                    {
                        var cfg = await db.GetOrCreateGuildConfig(guild).ConfigureAwait(false);
                        if (!cfg.LogBan.HasValue) return;


                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, (Addons.Database.Data.ModAction) ModAction.Unban);
                        var author = new EmbedAuthorBuilder
                        {
                            Name = $"Case ID: {caseId.Id} - {ActionType.UnBent} | {user.Username}#{user.Discriminator}"
                        };
                        var footer = new EmbedFooterBuilder
                        {
                            Text = $"User ID: {user.Id}"
                        };
                        var embed = new EmbedBuilder
                        {
                            Author = author,
                            Footer = footer,
                            Color = new Color(Color.Green.RawValue),
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = ModLogFieldBuilders(user)
                        };
                        var msg = await guild.GetTextChannel(cfg.LogBan.Value).SendEmbedAsync(embed);
                        caseId.MessageId = msg.Id;
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static Task MessageDeleted(Cacheable<IMessage, ulong> optMsg, ISocketMessageChannel ch)
        {
            var _ = Task.Run(async () =>
            {
                if (optMsg.HasValue && optMsg.Value.Author.IsBot) return;
                using (var db = new DbService())
                {
                    if (!(ch is SocketGuildChannel chx)) return;
                    var cfg = await db.GetOrCreateGuildConfig(chx.Guild).ConfigureAwait(false);
                    if (!cfg.LogMsg.HasValue) return;
                    if (!((optMsg.HasValue ? optMsg.Value : null) is IUserMessage msg)) return;
                    var author = new EmbedAuthorBuilder
                    {
                        Name = "Message Deleted"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = Color.Purple,
                        Timestamp = msg.Timestamp,
                        Description = $"{msg.Author.Mention} deleted a message in {(ch as ITextChannel)?.Mention}"
                    };
                    embed.AddField(x =>
                    {
                        x.Name = "Message";
                        x.IsInline = false;
                        x.Value = $"{msg.Content.Truncate(900)}";
                    });
                    try
                    {
                        embed.AddField(x =>
                        {
                            x.Name = "File";
                            x.IsInline = false;
                            x.Value = msg.Attachments.FirstOrDefault()?.Url;
                        });
                    }
                    catch
                    {
                        // Ignore
                    }

                    await chx.Guild.GetTextChannel(cfg.LogMsg.Value).SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg,
            ISocketMessageChannel channel)
        {
            var _ = Task.Run(async () =>
            {
                if (newMsg.Author.IsBot) return;
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(((SocketGuildChannel)channel).Guild);
                    if (!cfg.LogMsg.HasValue) return;
                    if (!((oldMsg.HasValue ? oldMsg.Value : null) is IUserMessage msg) || newMsg == null) return;
                    if (!(channel is ITextChannel chtx)) return;
                    if (msg.Author.IsBot && oldMsg.Value.Content == newMsg.Content) return;
                    if (oldMsg.Value.Content == newMsg.Content) return;
                    var author = new EmbedAuthorBuilder
                    {
                        Name = "Message Updated"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = Color.Purple,
                        Timestamp = newMsg.EditedTimestamp ?? newMsg.Timestamp,
                        Description = $"{newMsg.Author.Mention} updated a message in {chtx.Mention}"
                    };
                    embed.AddField(x =>
                    {
                        x.Name = "Updated Message:";
                        x.IsInline = true;
                        x.Value = $"{newMsg.Content.Truncate(900)}";
                    });
                    embed.AddField(x =>
                    {
                        x.Name = "Old Message:";
                        x.IsInline = true;
                        x.Value = $"{msg.Content.Truncate(900)}";
                    });
                    await (channel as SocketGuildChannel)?.Guild
                        .GetTextChannel(cfg.LogMsg.Value).SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static ILoggerFactory ConfigureLogging(ILoggerFactory factory)
        {
            factory.AddConsole();
            return factory;
        }

        private Task LogDiscord(LogMessage message)
        {
            var _ = Task.Run(async () =>
            {
                _discordLogger.Log(
                    LogLevelFromSeverity(message.Severity),
                    0,
                    message,
                    message.Exception,
                    (_1, _2) => message.ToString(prependTimestamp: false));
            });
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            var _ = Task.Run(async () =>
            {
                // Return an error message for async commands
                if (message.Exception is CommandException command)
                {
                    Console.WriteLine($"Error: {command.Message}");
                    var __ = command.Context.Client.GetApplicationInfoAsync().Result.Owner
                        .SendMessageAsync($"Error: {command.Message}\n" +
                                          $"{command.StackTrace}");
                }

                _commandsLogger.Log(
                    LogLevelFromSeverity(message.Severity),
                    0,
                    message,
                    message.Exception,
                    (_1, _2) => message.ToString(prependTimestamp: false));
            });
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
        {
            return (LogLevel)Math.Abs((int)severity - 5);
        }

        private static List<EmbedFieldBuilder> ModLogFieldBuilders(IMentionable user,
            string reason = null,
            TimeSpan? duration = null)
        {
            var userField = new EmbedFieldBuilder
            {
                Name = "User",
                Value = user.Mention,
                IsInline = true
            };
            var reasonField = new EmbedFieldBuilder
            {
                Name = "Reason",
                Value = reason ?? "N/A",
                IsInline = true
            };
            var modField = new EmbedFieldBuilder
            {
                Name = "Moderator",
                Value = "N/A",
                IsInline = true
            };
            var result = new List<EmbedFieldBuilder> { userField, modField, reasonField };
            if (duration == null) return result;
            var durationField = new EmbedFieldBuilder
            {
                Name = "Duration",
                Value = duration.Value.Humanize(),
                IsInline = true
            };
            result.Add(durationField);
            return result;
        }

        private async Task GetAuditLogData(SocketGuild guild, SocketUser user, Discord.ActionType type)
        {
            var auditlogs = (await guild.GetAuditLogsAsync(1).FlattenAsync()).FirstOrDefault(x => x.Action == type);
        }
    }

    public static class ActionType
    {
        public const string Gagged = "🔇Muted";
        public const string Ungagged = "🔊UnMuted";
        public const string Bent = "❌Banned";
        public const string UnBent = "✔UnBanned";
    }
}