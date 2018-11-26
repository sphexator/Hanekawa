using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Entities.LogEntities;
using Hanekawa.Extensions;
using Humanizer;
using ActionType = Hanekawa.Entities.ActionType;

namespace Hanekawa.Services.Logging.LoadBalance
{
    public class Tasks : IHanaService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public Tasks(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;

            _client.LeftGuild += GuildCleanup;
            Console.WriteLine("Task service loaded");
        }

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserBanned>> BanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserBanned>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<UserUnbanned>> UnBanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserUnbanned>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<UserJoined>> UserJoinedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserJoined>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<UserLeft>> UserLeftQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserLeft>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<MessageDeleted>> MessageDeletedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<MessageDeleted>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<MessageUpdated>> MessageUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<MessageUpdated>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<GuildUserUpdated>> GuildMemberUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<GuildUserUpdated>>();
        public ConcurrentDictionary<ulong, ConcurrentQueue<UserUpdated>> UserUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserUpdated>>();

        private Task GuildCleanup(SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                BanQueue.TryRemove(guild.Id, out var _);
                UnBanQueue.TryRemove(guild.Id, out var _);
                UserJoinedQueue.TryRemove(guild.Id, out var _);
                UserLeftQueue.TryRemove(guild.Id, out var _);
                MessageDeletedQueue.TryRemove(guild.Id, out var _);
                MessageUpdatedQueue.TryRemove(guild.Id, out var _);
                GuildMemberUpdatedQueue.TryRemove(guild.Id, out var _);
                UserUpdatedQueue.TryRemove(guild.Id, out var _);
            });
            return Task.CompletedTask;
        }

        public async Task ProcessJoinEvent(ulong guildId)
        {
            var queue = UserJoinedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserJoined>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;

                    var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogJoin.HasValue) return;
                    var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📥 {user.Mention} has joined (*{user.Id}*)\n" +
                                      $"Account Created: {user.CreatedAt.Humanize()}",

                        Color = Color.Green,
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Footer = new EmbedFooterBuilder {Text = $"Username: {user.Username}#{user.Discriminator}"}
                    };

                    await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessLeaveEvent(ulong guildId)
        {
            var queue = UserLeftQueue.GetOrAdd(guildId, new ConcurrentQueue<UserLeft>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;

                    var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
                    if (!cfg.LogJoin.HasValue) return;
                    var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                    if (ch == null) return;
                    var embed = new EmbedBuilder
                    {
                        Description = $"📤 {user.Mention} has left (*{user.Id}*)",
                        Timestamp = new DateTimeOffset(DateTime.UtcNow),
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder {Text = $"Username: {user.Username}#{user.Discriminator}"}
                    };

                    await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageDeletedEvent(ulong guildId)
        {
            var queue = MessageDeletedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageDeleted>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var messageDeleted))
                {
                    if (messageDeleted.OptMsg.HasValue && messageDeleted.OptMsg.Value.Author.IsBot) return;

                    if (!(messageDeleted.Channel is SocketGuildChannel chx)) return;
                    var cfg = await _db.GetOrCreateGuildConfig(chx.Guild).ConfigureAwait(false);
                    if (!cfg.LogMsg.HasValue) return;
                    var channel = chx.Guild.GetTextChannel(cfg.LogMsg.Value);
                    if (channel == null) return;
                    if (!messageDeleted.OptMsg.HasValue) return;
                    if (!(messageDeleted.OptMsg.Value is IUserMessage msg)) return;

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder {Name = "Message Deleted"},
                        Footer = new EmbedFooterBuilder {Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"},
                        Color = Color.Purple,
                        Timestamp = msg.Timestamp,
                        Description = $"{msg.Author.Mention} deleted a message in {(chx as ITextChannel)?.Mention}",
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

                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageUpdatedEvent(ulong guildId)
        {
            var queue = MessageUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var messageUpdated))
                {
                    if (messageUpdated.NewMessage.Author.IsBot) return;

                    if (!(messageUpdated.Channel is ITextChannel chtx)) return;
                    var cfg = await _db.GetOrCreateGuildConfig(chtx.Guild);
                    if (!cfg.LogMsg.HasValue) return;

                    if (!messageUpdated.OldMessage.HasValue) return;
                    if (!(messageUpdated.OldMessage.Value is IUserMessage msg)) return;
                    var channel = await chtx.Guild.GetTextChannelAsync(cfg.LogMsg.Value);

                    if (channel == null) return;
                    if (msg.Author.IsBot &&
                        messageUpdated.OldMessage.Value.Content == messageUpdated.NewMessage.Content) return;
                    if (messageUpdated.OldMessage.Value.Content == messageUpdated.NewMessage.Content) return;

                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder {Name = "Message Updated"},
                        Footer = new EmbedFooterBuilder {Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"},
                        Color = Color.Purple,
                        Timestamp = messageUpdated.NewMessage.EditedTimestamp ?? messageUpdated.NewMessage.Timestamp,
                        Description = $"{messageUpdated.NewMessage.Author.Mention} updated a message in {chtx.Mention}",
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                IsInline = true, Name = "Updated Message:",
                                Value = messageUpdated.NewMessage.Content.Truncate(900)
                            },
                            new EmbedFieldBuilder
                                {IsInline = true, Name = "Old Message:", Value = msg.Content.Truncate(900)}
                        }
                    };
                    await channel.SendEmbedAsync(embed);


                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserBannedEvent(ulong guildId)
        {
            var queue = BanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserBanned>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userBanned))
                {
                    try
                    {
                        var cfg = await _db.GetOrCreateGuildConfig(userBanned.Guild).ConfigureAwait(false);
                        if (!cfg.LogBan.HasValue) return;
                        var ch = userBanned.Guild.GetTextChannel(cfg.LogBan.Value);
                        if (ch == null) return;

                        var caseId = await _db.CreateCaseId(userBanned.User, userBanned.Guild, DateTime.UtcNow,
                            (ModAction) Data.Constants.ModAction.Ban);
                        var embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name =
                                    $"Case ID: {caseId.Id} - {ActionType.Bent} | {userBanned.User.Username}#{userBanned.User.Discriminator}"
                            },
                            Footer = new EmbedFooterBuilder {Text = $"User ID: {userBanned.User.Id}"},
                            Color = Color.Red,
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = new List<EmbedFieldBuilder>().ModLogFieldBuilders(userBanned.User)
                        };
                        var msg = await ch.SendEmbedAsync(embed);
                        caseId.MessageId = msg.Id;
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUnbannedEvent(ulong guildId)
        {
            var queue = UnBanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUnbanned>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userUnbanned))
                {
                    try
                    {
                        var cfg = await _db.GetOrCreateGuildConfig(userUnbanned.Guild).ConfigureAwait(false);
                        if (!cfg.LogBan.HasValue) return;
                        var ch = userUnbanned.Guild.GetTextChannel(cfg.LogBan.Value);
                        if (ch == null) return;

                        var caseId = await _db.CreateCaseId(userUnbanned.User, userUnbanned.Guild, DateTime.UtcNow,
                            (ModAction) Data.Constants.ModAction.Unban);
                        var embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name =
                                    $"Case ID: {caseId.Id} - {ActionType.UnBent} | {userUnbanned.User.Username}#{userUnbanned.User.Discriminator}"
                            },
                            Footer = new EmbedFooterBuilder {Text = $"User ID: {userUnbanned.User.Id}"},
                            Color = Color.Green,
                            Timestamp = new DateTimeOffset(DateTime.UtcNow),
                            Fields = new List<EmbedFieldBuilder>().ModLogFieldBuilders(userUnbanned.User)
                        };
                        var msg = await ch.SendEmbedAsync(embed);
                        caseId.MessageId = msg.Id;
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUpdatedEvent(ulong guildId)
        {
            var queue = UserUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userUpdated))
                {
                    if (!(userUpdated.NewUser is SocketGuildUser gusr)) return;
                    var cfg = await _db.GetOrCreateGuildConfig(gusr.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var ch = gusr.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (ch == null) return;

                    var embed = new EmbedBuilder {Color = Color.Purple};
                    if (userUpdated.OldUser.Username != userUpdated.NewUser.Username)
                    {
                        var updated = userUpdated;
                        embed.WithTitle("Username Change")
                            .WithDescription(
                                $"{userUpdated.OldUser.Username}#{updated.OldUser.Discriminator} || {userUpdated.OldUser.Id}")
                            .AddField(x =>
                                x.WithName("Old Name").WithValue($"{updated.OldUser.Username}").WithIsInline(true))
                            .AddField(x =>
                                x.WithName("New Name").WithValue($"{updated.NewUser.Username}").WithIsInline(true));
                    }
                    else if (userUpdated.OldUser.AvatarId != userUpdated.NewUser.AvatarId)
                    {
                        embed.WithTitle("Avatar Change")
                            .WithDescription(
                                $"{userUpdated.OldUser.Username}#{userUpdated.OldUser.Discriminator} | {userUpdated.OldUser.Id}");

                        if (Uri.IsWellFormedUriString(userUpdated.OldUser.GetAvatarUrl(ImageFormat.Auto, 1024),
                            UriKind.Absolute))
                            embed.WithThumbnailUrl(userUpdated.OldUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                        if (Uri.IsWellFormedUriString(userUpdated.NewUser.GetAvatarUrl(ImageFormat.Auto, 1024),
                            UriKind.Absolute))
                            embed.WithImageUrl(userUpdated.NewUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                    }
                    else
                    {
                        return;
                    }

                    await ch.SendMessageAsync(null, false, embed.Build());

                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessGuildUserUpdatedEvent(ulong guildId)
        {
            var queue = GuildMemberUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<GuildUserUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var guildUserUpdated))
                {
                    var cfg = await _db.GetOrCreateGuildConfig(guildUserUpdated.NewUser.Guild);
                    if (!cfg.LogAvi.HasValue) return;
                    var ch = guildUserUpdated.NewUser.Guild.GetTextChannel(cfg.LogAvi.Value);
                    if (ch == null) return;

                    var embed = new EmbedBuilder
                    {
                        Title =
                            $"{guildUserUpdated.OldUser.Username}#{guildUserUpdated.OldUser.Discriminator} | {guildUserUpdated.OldUser.Id}",
                        Footer = new EmbedFooterBuilder {IconUrl = guildUserUpdated.NewUser.GetAvatarUrl(), Text = ""}
                    };

                    if (guildUserUpdated.OldUser.Nickname != guildUserUpdated.NewUser.Nickname)
                    {
                        var updated = guildUserUpdated;
                        embed.WithAuthor(x => x.WithName("Nick Change"))
                            .AddField(
                                x => x.WithName("Old Nick")
                                    .WithValue($"{updated.OldUser.Nickname}#{updated.OldUser.Discriminator}"))
                            .AddField(
                                x => x.WithName("New Nick")
                                    .WithValue($"{updated.OldUser.Nickname}#{updated.OldUser.Discriminator}"));
                        await ch.SendMessageAsync(null, false, embed.Build()).ConfigureAwait(false);
                    }
                    else if (!guildUserUpdated.OldUser.Roles.SequenceEqual(guildUserUpdated.NewUser.Roles))
                    {
                        if (guildUserUpdated.OldUser.Roles.Count < guildUserUpdated.NewUser.Roles.Count)
                        {
                            var updated = guildUserUpdated;
                            var roleDiffer = guildUserUpdated.NewUser.Roles
                                .Where(x => !updated.OldUser.Roles.Contains(x)).Select(x => x.Name);
                            embed.WithAuthor(x => x.WithName("User role added"))
                                .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                        }
                        else if (guildUserUpdated.OldUser.Roles.Count > guildUserUpdated.NewUser.Roles.Count)
                        {
                            var updated = guildUserUpdated;
                            var roleDiffer = guildUserUpdated.OldUser.Roles
                                .Where(x => !updated.NewUser.Roles.Contains(x)).Select(x => x.Name);
                            embed.WithAuthor(x => x.WithName("User role removed"))
                                .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                        }

                        await ch.SendMessageAsync(null, false, embed.Build());
                    }

                    lastRequest = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }
    }
}