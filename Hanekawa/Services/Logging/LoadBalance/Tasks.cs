using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Log;
using Hanekawa.Extensions;
using Humanizer;
using ActionType = Hanekawa.Entities.ActionType;

namespace Hanekawa.Services.Logging.LoadBalance
{
    public class Tasks
    {
        private readonly DiscordSocketClient _client;

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

        public Tasks(DiscordSocketClient client)
        {
            _client = client;

            _client.LeftGuild += GuildCleanup;
        }

        private Task GuildCleanup(SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                BanQueue.TryRemove(guild.Id, out var banQueue);
                UnBanQueue.TryRemove(guild.Id, out var unbanQueue);
                UserJoinedQueue.TryRemove(guild.Id, out var userJoinedQueue);
                UserLeftQueue.TryRemove(guild.Id, out var userLeftQueue);
                MessageDeletedQueue.TryRemove(guild.Id, out var messageDeletedQueue);
                MessageUpdatedQueue.TryRemove(guild.Id, out var messageUpdatedQueue);
                GuildMemberUpdatedQueue.TryRemove(guild.Id, out var guildMemberUpdatedQueue);
                UserUpdatedQueue.TryRemove(guild.Id, out var userUpdatedQueue);
            });
            return Task.CompletedTask;
        }

        public async Task ProcessJoinEvent(ulong guildId)
        {
            var queue = UserJoinedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserJoined>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;
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
                        lastRequest = DateTime.UtcNow;
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessLeaveEvent(ulong guildId)
        {
            var queue = UserLeftQueue.GetOrAdd(guildId, new ConcurrentQueue<UserLeft>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;
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
                        lastRequest = DateTime.UtcNow;
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageDeletedEvent(ulong guildId)
        {
            var queue = MessageDeletedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageDeleted>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {

                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageUpdatedEvent(ulong guildId)
        {
            var queue = MessageUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageUpdated>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {

                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserBannedEvent(ulong guildId)
        {
            var queue = BanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserBanned>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userBanned))
                {
                    using (var db = new DbService())
                    {
                        try
                        {
                            var cfg = await db.GetOrCreateGuildConfig(userBanned.Guild).ConfigureAwait(false);
                            if (!cfg.LogBan.HasValue) return;
                            var ch = userBanned.Guild.GetTextChannel(cfg.LogBan.Value);
                            if (ch == null) return;

                            var caseId = await db.CreateCaseId(userBanned.User, userBanned.Guild, DateTime.UtcNow,
                                (ModAction)Data.Constants.ModAction.Ban);
                            var embed = new EmbedBuilder
                            {
                                Author = new EmbedAuthorBuilder
                                {
                                    Name =
                                        $"Case ID: {caseId.Id} - {ActionType.Bent} | {userBanned.User.Username}#{userBanned.User.Discriminator}"
                                },
                                Footer = new EmbedFooterBuilder { Text = $"User ID: {userBanned.User.Id}" },
                                Color = Color.Red,
                                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                                // Fields = ModLogFieldBuilders(user) - TODO: ADD
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
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUnbannedEvent(ulong guildId)
        {
            var queue = UnBanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUnbanned>()); 
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userUnbanned))
                {
                    using (var db = new DbService())
                    {
                        try
                        {
                            var cfg = await db.GetOrCreateGuildConfig(userUnbanned.Guild).ConfigureAwait(false);
                            if (!cfg.LogBan.HasValue) return;
                            var ch = userUnbanned.Guild.GetTextChannel(cfg.LogBan.Value);
                            if (ch == null) return;

                            var caseId = await db.CreateCaseId(userUnbanned.User, userUnbanned.Guild, DateTime.UtcNow,
                                (ModAction)Data.Constants.ModAction.Unban);
                            var embed = new EmbedBuilder
                            {
                                Author = new EmbedAuthorBuilder
                                {
                                    Name =
                                        $"Case ID: {caseId.Id} - {ActionType.UnBent} | {userUnbanned.User.Username}#{userUnbanned.User.Discriminator}"
                                },
                                Footer = new EmbedFooterBuilder { Text = $"User ID: {userUnbanned.User.Id}" },
                                Color = Color.Green,
                                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                                // Fields = ModLogFieldBuilders(user) TODO: ADD
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
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUpdatedEvent(ulong guildId)
        {
            var queue = UserUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUpdated>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {

                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessGuildUserUpdatedEvent(ulong guildId)
        {
            var queue = GuildMemberUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<GuildUserUpdated>());
            var worker = true;
            while (worker)
            {
                var lastRequest = DateTime.UtcNow;
                while (queue.TryDequeue(out var userJoined))
                {

                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(18) <= DateTime.UtcNow) worker = false;
            }
        }
    }
}