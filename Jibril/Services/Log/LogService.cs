using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Data.Constants;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;
using Hanekawa.Services.Entities;
using Humanizer;
using Microsoft.Extensions.Logging;

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

        public LogService(DiscordSocketClient client, CommandService commands, ILoggerFactory loggerFactory,
            ModerationService moderationService, MuteService muteService)
        {
            _client = client;
            _commands = commands;
            _moderationService = moderationService;
            _muteService = muteService;

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

            _muteService.UserMuted += UserMuted;
            _muteService.UserTimedMuted += UserTimedMute;
            _muteService.UserUnmuted += UserUnmuted;

            _moderationService.AutoModPermLog += AutoModPermLog;
            _moderationService.AutoModTimedLog += AutoModTimedLog;
        }

        private static Task AutoModTimedLog(SocketGuildUser user, ModerationService.AutoModActionType type, TimeSpan timer, string msg)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogBan.HasValue) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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
                    var duration = new EmbedFieldBuilder
                    {
                        Name = "Duration",
                        Value = timer.Humanize(),
                        IsInline = false
                    };
                    var message = new EmbedFieldBuilder
                    {
                        Name = "Message",
                        Value = msg.Truncate(999),
                        IsInline = false
                    };
                    var result = new List<EmbedFieldBuilder> {userField, modField, reasonField, duration, message};

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

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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
                    var result = new List<EmbedFieldBuilder> {userField, modField, reasonField, message};

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

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow, ModAction.Mute);
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
                        Timestamp = new DateTimeOffset(DateTime.UtcNow)
                    };
                    if (!user.IsBot)
                    {
                        var userdata = await db.GetOrCreateUserData(user).ConfigureAwait(false);
                        embed.Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator} - Lvl: {userdata.Level}" };
                    }
                    else
                    {
                        embed.Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator}" };
                    }
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
                        Color = new Color(Color.Red.RawValue)
                    };
                    if (!user.IsBot)
                    {
                        var userdata = await db.GetOrCreateUserData(user).ConfigureAwait(false);
                        embed.Footer = new EmbedFooterBuilder{Text = $"Username: {user.Username}#{user.Discriminator} - Level: {userdata.Level}"};
                    }
                    else
                    {
                        embed.Footer = new EmbedFooterBuilder{Text = $"Username: {user.Username}#{user.Discriminator}"};
                    }
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

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Ban);
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
                            Color = new Color(Color.Red.RawValue),
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = ModLogFieldBuilders(user as IGuildUser)
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


                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow, ModAction.Unban);
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
                            Fields = ModLogFieldBuilders(user.Mention)
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
                using (var db = new DbService())
                {
                    if (!(ch is SocketGuildChannel chx)) return;
                    var cfg = await db.GetOrCreateGuildConfig(chx.Guild).ConfigureAwait(false);
                    if (!cfg.LogMsg.HasValue) return;
                    if (optMsg.Value.Author.IsBot) return;
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
                        Color = new Color(Color.DarkPurple.RawValue),
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
                        Color = new Color(Color.DarkPurple.RawValue),
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
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            // Return an error message for async commands
            if (message.Exception is CommandException command)
            {
                Console.WriteLine($"Error: {command.Message}");
                var _ = command.Context.Client.GetApplicationInfoAsync().Result.Owner
                    .SendMessageAsync($"Error: {command.Message}\n" +
                                      $"{command.StackTrace}");
            }
            _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
        {
            return (LogLevel) Math.Abs((int) severity - 5);
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
            var result = new List<EmbedFieldBuilder> {userField, modField, reasonField};
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

        private static List<EmbedFieldBuilder> ModLogFieldBuilders(string user,
            string reason = null,
            TimeSpan? duration = null)
        {
            var userField = new EmbedFieldBuilder
            {
                Name = "User",
                Value = user,
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
    }

    public static class ActionType
    {
        public const string Gagged = "🔇Muted";
        public const string Ungagged = "🔊UnMuted";
        public const string Bent = "❌Banned";
        public const string UnBent = "✔UnBanned";
    }
}