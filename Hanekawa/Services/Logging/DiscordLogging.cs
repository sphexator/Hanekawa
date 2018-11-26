using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.LogEntities;
using Hanekawa.Extensions;
using Hanekawa.Services.Administration;
using Hanekawa.Services.AutoModerator;
using Hanekawa.Services.Logging.LoadBalance;
using Humanizer;
using ActionType = Hanekawa.Entities.ActionType;

namespace Hanekawa.Services.Logging
{
    public class DiscordLogging
    {
        private readonly LogLoadBalancer _balancer;
        private readonly DiscordSocketClient _client;
        private readonly ModerationService _moderationService;
        private readonly MuteService _muteService;
        private readonly NudeScoreService _nudeService;
        private readonly WarnService _warnService;

        public DiscordLogging(DiscordSocketClient client, ModerationService moderationService, MuteService muteService,
            WarnService warnService, NudeScoreService nudeService, LogLoadBalancer balancer)
        {
            _client = client;

            _moderationService = moderationService;
            _muteService = muteService;
            _warnService = warnService;
            _nudeService = nudeService;
            _balancer = balancer;

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
            _moderationService.AutoModFilter += AutoModFilterLog;
            _nudeService.AutoModFilter += AutoModToxicityFilter;

            _warnService.UserWarned += UserWarnLog;
            _warnService.UserMuted += UserMuteWarnLog;
            Console.WriteLine("Discord logging service loaded");
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
                        Author = new EmbedAuthorBuilder {Name = "User Mute Warned"},
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = $"{user.Mention}"},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = staff},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason.Truncate(700)},
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Duration", Value = $"{duration.Humanize()} ({duration})"}
                        },
                        Footer = new EmbedFooterBuilder
                            {Text = $"Username: {user.Username}#{user.DiscriminatorValue} ({user.Id})"}
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
                        Author = new EmbedAuthorBuilder {Name = "User Warned"},
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = staff},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason.Truncate(700)}
                        },
                        Footer = new EmbedFooterBuilder
                            {Text = $"Username: {user.Username}#{user.DiscriminatorValue} ({user.Id})"}
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
                Author = new EmbedAuthorBuilder {Name = $"{action} - {user.Username}#{user.DiscriminatorValue}"},
                Color = Color.Red,
                Description = content,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                    new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = "Auto-moderator"},
                    new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = reason}
                },
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                Footer = new EmbedFooterBuilder {Text = $"User ID: {user.Id}"}
            };
            if (timer.HasValue) embed.AddField("Duration", timer.Value.Humanize(), true);
            await channel.SendEmbedAsync(embed);
        }

        private static Task AutoModToxicityFilter(SocketGuildUser user, ModerationService.AutoModActionType type,
            string content, double score, double tolerance)
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
                        Author = new EmbedAuthorBuilder
                            {Name = $"Toxicity - {user.Username}#{user.DiscriminatorValue}"},
                        Color = Color.Red,
                        Description = content,
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder {IsInline = true, Name = "User", Value = user.Mention},
                            new EmbedFieldBuilder {IsInline = true, Name = "Staff", Value = "Auto-moderator"},
                            new EmbedFieldBuilder {IsInline = true, Name = "Reason", Value = "Toxicity"},
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Score", Value = $"{score} (higher then {tolerance})"}
                        },
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder {Text = $"User ID: {user.Id}"}
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
                            {Name = $"{user.Username}#{user.DiscriminatorValue} | {ActionType.Ungagged}"},
                        Description = $"{user.Mention}",
                        Color = Color.Green,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder {Text = $"User ID: {user.Id}"}
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
                        (ModAction) Data.Constants.ModAction.Mute);
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name =
                                $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
                        },
                        Color = new Color(Color.Red.RawValue),
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = new List<EmbedFieldBuilder>().ModLogFieldBuilders(user)
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
                        (ModAction) Data.Constants.ModAction.Mute);
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name =
                                $"Case ID: {caseId.Id} - {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
                        },
                        Footer = new EmbedFooterBuilder {Text = $"User ID: {user.Id}"},
                        Color = Color.Red,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Fields = new List<EmbedFieldBuilder>().ModLogFieldBuilders(user)
                    };
                    var msg = await ch.SendEmbedAsync(embed);
                    caseId.MessageId = msg.Id;
                    caseId.ModId = staff.Id;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            var _ = Task.Run(() =>
            {
                var data = new UserJoined {User = user};
                _balancer.Add(LogType.UserJoined, data);
            });
            return Task.CompletedTask;
        }

        private Task UserLeft(SocketGuildUser user)
        {
            var _ = Task.Run(() =>
            {
                var data = new UserLeft {User = user};
                _balancer.Add(LogType.UserLeft, data);
            });
            return Task.CompletedTask;
        }

        private Task Banned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                var data = new UserBanned
                {
                    Guild = guild,
                    User = user
                };
                _balancer.Add(LogType.UserBanned, data);
            });
            return Task.CompletedTask;
        }

        private Task Unbanned(SocketUser user, SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                var data = new UserUnbanned
                {
                    Guild = guild,
                    User = user
                };
                _balancer.Add(LogType.UserUnbanned, data);
            });
            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> optMsg, ISocketMessageChannel ch)
        {
            var _ = Task.Run(() =>
            {
                var data = new MessageDeleted
                {
                    OptMsg = optMsg,
                    Channel = ch
                };
                _balancer.Add(LogType.MessageDeleted, data);
            });
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg,
            ISocketMessageChannel ch)
        {
            var _ = Task.Run(() =>
            {
                var data = new MessageUpdated
                {
                    OldMessage = oldMsg,
                    NewMessage = newMsg,
                    Channel = ch
                };
                _balancer.Add(LogType.MessageUpdated, data);
            });
            return Task.CompletedTask;
        }

        private Task UserUpdated(SocketUser oldUsr, SocketUser newUsr)
        {
            var _ = Task.Run(() =>
            {
                var data = new UserUpdated
                {
                    OldUser = oldUsr,
                    NewUser = newUsr
                };
                _balancer.Add(LogType.UserUpdated, data);
            });
            return Task.CompletedTask;
        }

        private Task GuildUserUpdated(SocketGuildUser oldUsr, SocketGuildUser newUsr)
        {
            var _ = Task.Run(() =>
            {
                var data = new GuildUserUpdated
                {
                    OldUser = oldUsr,
                    NewUser = newUsr
                };
                _balancer.Add(LogType.GuildUserUpdated, data);
            });
            return Task.CompletedTask;
        }
    }
}