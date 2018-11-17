using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;
using Humanizer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hanekawa.Addons.EventQueue;
using ActionType = Hanekawa.Entities.ActionType;

namespace Hanekawa.Services.Log
{
    public class DiscordLogging
    {
        private readonly DiscordSocketClient _client;
        private readonly ModerationService _moderationService;
        private readonly MuteService _muteService;
        private readonly WarnService _warnService;
        private readonly NudeScoreService _nudeService;
        private readonly EventQueue<DiscordSocketClient> _queue;
        private readonly CancellationToken _ctoken;

        private ConcurrentDictionary<ulong, Task> UserBannedGuildTasks { get; } =
            new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> BanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> UserUnBannedGuildTasks { get; } =
            new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> UnBanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> UserJoinedTasks { get; } = new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> UserJoinedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> UserLeftTasks { get; } = new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> UserLeftQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> MessageDeletedTasks { get; } =
            new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> MessageDeletedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> MessageUpdatedTasks { get; } = new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> MessageUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> GuildMemberUpdatedTasks { get; } = new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> GuildMemberUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        private ConcurrentDictionary<ulong, Task> UserUpdatedTasks { get; } = new ConcurrentDictionary<ulong, Task>();
        private ConcurrentDictionary<ulong, ConcurrentQueue<string>> UserUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<string>>();

        public DiscordLogging(DiscordSocketClient client, ModerationService moderationService, MuteService muteService, WarnService warnService, NudeScoreService nudeService)
        {
            _client = client;
            _queue = new EventQueue<DiscordSocketClient>(client);

            _moderationService = moderationService;
            _muteService = muteService;
            _warnService = warnService;
            _nudeService = nudeService;

            _client.UserBanned += Banned;
            _client.UserUnbanned += Unbanned;
            _queue.Register(nameof(_client.UserJoined)); // _client.UserJoined += UserJoined;
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
            _moderationService.AutoModFilter += AutoModFilterLog;
            _nudeService.AutoModFilter += AutoModToxicityFilter;

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

                    var embed = new EmbedBuilder { Color = Color.Purple };
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
                    else
                    {
                        return;
                    }

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

                    var embed = new EmbedBuilder
                    {
                        Title = $"{oldUsr.Username}#{oldUsr.Discriminator} | {oldUsr.Id}",
                        Footer = new EmbedFooterBuilder { IconUrl = newUsr.GetAvatarUrl(), Text = "" }
                    };

                    if (oldUsr.Nickname != newUsr.Nickname)
                    {
                        embed.WithAuthor(x => x.WithName("Nick Change"))
                            .AddField(
                                x => x.WithName("Old Nick").WithValue($"{oldUsr.Nickname}#{oldUsr.Discriminator}"))
                            .AddField(
                                x => x.WithName("New Nick").WithValue($"{newUsr.Nickname}#{newUsr.Discriminator}"));
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
                    var ch = user.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Purple,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Author = new EmbedAuthorBuilder { Name = "User Mute Warned" },
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = $"{user.Mention}"},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = staff},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason.Truncate(700)},
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Duration", Value = $"{duration.Humanize()} ({duration})"}
                        },
                        Footer = new EmbedFooterBuilder
                        { Text = $"Username: {user.Username}#{user.DiscriminatorValue} ({user.Id})" }
                    };
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
                    var ch = user.Guild.GetTextChannel(cfg.LogWarn.Value);
                    if (ch == null) return;

                    var embed = new EmbedBuilder
                    {
                        Color = Color.Purple,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Author = new EmbedAuthorBuilder { Name = "User Warned" },
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = staff},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason.Truncate(700)}
                        },
                        Footer = new EmbedFooterBuilder
                        { Text = $"Username: {user.Username}#{user.DiscriminatorValue} ({user.Id})" }
                    };
                    await ch.SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task AutoModFilterLog(SocketGuildUser user, ModerationService.AutoModActionType type, int amount,
            string content)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    ITextChannel channel;
                    if (cfg.LogAutoMod.HasValue) channel = user.Guild.GetTextChannel(cfg.LogAutoMod.Value);
                    else if (cfg.LogBan.HasValue) channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    else return;
                    if (channel == null) return;
                    await AutoModLog(user, channel, type, content, $"{type.ToString()} - Amount: {amount}",
                        ActionType.Deleted);
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
                    ITextChannel channel;
                    if (cfg.LogBan.HasValue) channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    else if (cfg.LogAutoMod.HasValue) channel = user.Guild.GetTextChannel(cfg.LogAutoMod.Value);
                    else return;
                    if (channel == null) return;
                    await AutoModLog(user, channel, type, msg, type.ToString(), ActionType.Gagged, timer);
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
                    ITextChannel channel;
                    if (!cfg.LogBan.HasValue) channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    else if (cfg.LogAutoMod.HasValue) channel = user.Guild.GetTextChannel(cfg.LogAutoMod.Value);
                    else return;
                    if (channel == null) return;
                    await AutoModLog(user, channel, type, msg, type.ToString(), ActionType.Gagged);
                }
            });
            return Task.CompletedTask;
        }

        private static async Task AutoModLog(IUser user, IMessageChannel channel,
            ModerationService.AutoModActionType type, string content, string reason, string action,
            TimeSpan? timer = null)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder { Name = $"{action} - {user.Username}#{user.DiscriminatorValue}" },
                Color = Color.Red,
                Description = content,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                    new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = "Auto-moderator"},
                    new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason}
                },
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" }
            };
            if (timer.HasValue) embed.AddField("Duration", timer.Value.Humanize(), true);
            await channel.SendEmbedAsync(embed);
        }

        private static Task AutoModToxicityFilter(SocketGuildUser user, ModerationService.AutoModActionType type, string content, double score, double tolerance)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    ITextChannel channel;
                    if (cfg.LogAutoMod.HasValue) channel = user.Guild.GetTextChannel(cfg.LogAutoMod.Value);
                    else if (cfg.LogBan.HasValue) channel = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    else return;
                    if (channel == null) return;
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = $"Toxicity - {user.Username}#{user.DiscriminatorValue}" },
                        Color = Color.Red,
                        Description = content,
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = "Auto-moderator"},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = "Toxicity"},
                            new EmbedFieldBuilder {IsInline = true, Name = "Score", Value = $"{score} (higher then {tolerance})"}
                        },
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" }
                    };
                    await channel.SendEmbedAsync(embed);
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
                    var ch = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    if (ch == null) return;

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        { Name = $"{user.Username}#{user.DiscriminatorValue} | {ActionType.Ungagged}" },
                        Description = $"{user.Mention}",
                        Color = Color.Green,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" }
                    };
                    await ch.SendEmbedAsync(embed);
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
                    var ch = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    if (ch == null) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow,
                        (ModAction)Data.Constants.ModAction.Mute);
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name =
                                $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
                        },
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = ModLogFieldBuilders(user, null, timer),
                        Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" }
                    };
                    var msg = await ch.SendEmbedAsync(embed);
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
                    var ch = user.Guild.GetTextChannel(cfg.LogBan.Value);
                    if (ch == null) return;

                    var caseId = await db.CreateCaseId(user, user.Guild, DateTime.UtcNow,
                        (ModAction)Data.Constants.ModAction.Mute);
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name =
                                $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
                        },
                        Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                        Color = Color.Red,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = ModLogFieldBuilders(user)
                    };
                    var msg = await ch.SendEmbedAsync(embed);
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
                    var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📥 {user.Mention} has joined (*{user.Id}*)\n" +
                                      $"Account Created: {user.CreatedAt.Humanize()}",

                        Color = Color.Green,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator}" }
                    };

                    await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
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
                    var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📤 {user.Mention} has left (*{user.Id}*)",
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder { Text = $"Username: {user.Username}#{user.Discriminator}" }
                    };

                    await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
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
                        var ch = guild.GetTextChannel(cfg.LogBan.Value);
                        if (ch == null) return;

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow,
                            (ModAction)Data.Constants.ModAction.Ban);
                        var embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name =
                                    $"Case ID: {caseId.Id} - {ActionType.Bent} | {user.Username}#{user.Discriminator}"
                            },
                            Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                            Color = Color.Red,
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = ModLogFieldBuilders(user)
                        };
                        var msg = await ch.SendEmbedAsync(embed);
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
                        var ch = guild.GetTextChannel(cfg.LogBan.Value);
                        if (ch == null) return;

                        var caseId = await db.CreateCaseId(user, guild, DateTime.UtcNow,
                            (ModAction)Data.Constants.ModAction.Unban);
                        var embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name =
                                    $"Case ID: {caseId.Id} - {ActionType.UnBent} | {user.Username}#{user.Discriminator}"
                            },
                            Footer = new EmbedFooterBuilder { Text = $"User ID: {user.Id}" },
                            Color = Color.Green,
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = ModLogFieldBuilders(user)
                        };
                        var msg = await ch.SendEmbedAsync(embed);
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
                    var channel = chx.Guild.GetTextChannel(cfg.LogMsg.Value);
                    if (channel == null) return;
                    if (!optMsg.HasValue) return;
                    if (!(optMsg.Value is IUserMessage msg)) return;

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = "Message Deleted" },
                        Footer = new EmbedFooterBuilder { Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}" },
                        Color = Color.Purple,
                        Timestamp = msg.Timestamp,
                        Description = $"{msg.Author.Mention} deleted a message in {(ch as ITextChannel)?.Mention}",
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                                {IsInline = false, Name = "Message", Value = msg.Content.Truncate(900)}
                        }
                    };
                    if (msg.Attachments.Count > 0)
                        embed.AddField(x =>
                        {
                            x.Name = "File";
                            x.IsInline = false;
                            x.Value = msg.Attachments.First().Url;
                        });

                    await channel.SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg,
            ISocketMessageChannel ch)
        {
            var _ = Task.Run(async () =>
            {
                if (newMsg.Author.IsBot) return;
                using (var db = new DbService())
                {
                    if (!(ch is ITextChannel chtx)) return;
                    var cfg = await db.GetOrCreateGuildConfig(chtx.Guild);
                    if (!cfg.LogMsg.HasValue) return;

                    if (!oldMsg.HasValue) return;
                    if (!(oldMsg.Value is IUserMessage msg)) return; var channel = await chtx.Guild.GetTextChannelAsync(cfg.LogMsg.Value);

                    if (channel == null) return;
                    if (msg.Author.IsBot && oldMsg.Value.Content == newMsg.Content) return;
                    if (oldMsg.Value.Content == newMsg.Content) return;

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = "Message Updated" },
                        Footer = new EmbedFooterBuilder { Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}" },
                        Color = Color.Purple,
                        Timestamp = newMsg.EditedTimestamp ?? newMsg.Timestamp,
                        Description = $"{newMsg.Author.Mention} updated a message in {chtx.Mention}",
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Updated Message:", Value = newMsg.Content.Truncate(900)},
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Old Message:", Value = msg.Content.Truncate(900)}
                        }
                    };
                    await channel.SendEmbedAsync(embed);
                }
            });
            return Task.CompletedTask;
        }

        private static List<EmbedFieldBuilder> ModLogFieldBuilders(IMentionable user,
            string reason = null,
            TimeSpan? duration = null)
        {
            var result = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                new EmbedFieldBuilder {IsInline = true, Name = "Moderator", Value = "N/A"},
                new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason ?? "N/A"}
            };
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
            await Task.Delay(1000);
            var auditlogs = (await guild.GetAuditLogsAsync(1).FlattenAsync()).FirstOrDefault(x => x.Action == type);
        }
    }
}
