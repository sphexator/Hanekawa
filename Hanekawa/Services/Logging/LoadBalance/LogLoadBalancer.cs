using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Entities.LogEntities;

namespace Hanekawa.Services.Logging.LoadBalance
{
    public class LogLoadBalancer : IHanaService
    {
        private readonly DbService _db;
        private readonly Tasks _tasks;

        public LogLoadBalancer(Tasks tasks, DbService db)
        {
            _tasks = tasks;
            _db = db;
            Console.WriteLine("Log load balancer service started");
        }

        // Collections of tasks per event per guild
        private ConcurrentDictionary<ulong, Task> UserBannedGuildTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> UserUnBannedGuildTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> UserJoinedTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> UserLeftTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> MessageDeletedTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> MessageUpdatedTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> GuildMemberUpdatedTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        private ConcurrentDictionary<ulong, Task> UserUpdatedTasks { get; }
            = new ConcurrentDictionary<ulong, Task>();

        public void Add(LogType type, object entity)
        {
            switch (type)
            {
                case LogType.GuildUserUpdated:
                    var gUserUpdated = (GuildUserUpdated) entity;
                    _ = HandleGuildUserUpdated(gUserUpdated);
                    break;
                case LogType.UserUpdated:
                    var userUpdated = (UserUpdated) entity;
                    _ = HandleUserUpdated(userUpdated);
                    break;
                case LogType.MessageDeleted:
                    var messageDeleted = (MessageDeleted) entity;
                    _ = HandleMessageDeleted(messageDeleted);
                    break;
                case LogType.MessageUpdated:
                    var messageUpdated = (MessageUpdated) entity;
                    _ = HandleMessageUpdated(messageUpdated);
                    break;
                case LogType.UserBanned:
                    var userBanned = (UserBanned) entity;
                    _ = HandleUserBanned(userBanned);
                    break;
                case LogType.UserUnbanned:
                    var userUnbanned = (UserUnbanned) entity;
                    _ = HandleUserUnbanned(userUnbanned);
                    break;
                case LogType.UserJoined:
                    var userJoined = (UserJoined) entity;
                    _ = HandleUserJoined(userJoined);
                    break;
                case LogType.UserLeft:
                    var userLeft = (UserLeft) entity;
                    _ = HandleUserLeft(userLeft);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private async Task HandleUserJoined(UserJoined usr)
        {
            var user = usr.User;
            var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
            if (!cfg.LogJoin.HasValue) return;
            var queue = _tasks.UserJoinedQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserJoined>());
            queue.Enqueue(usr);
            if (UserJoinedTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = UserJoinedTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessJoinEvent,
                        (key, old) => _tasks.ProcessJoinEvent(user.Guild.Id));
            }
            else
            {
                UserJoinedTasks.TryAdd(user.Guild.Id, _tasks.ProcessJoinEvent(user.Guild.Id));
            }
        }

        private async Task HandleUserLeft(UserLeft usr)
        {
            var user = usr.User;
            var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
            if (!cfg.LogJoin.HasValue) return;

            var queue = _tasks.UserLeftQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserLeft>());
            queue.Enqueue(usr);
            if (UserLeftTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = UserLeftTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessLeaveEvent,
                        (key, old) => _tasks.ProcessLeaveEvent(user.Guild.Id));
            }
            else
            {
                UserLeftTasks.TryAdd(user.Guild.Id, _tasks.ProcessLeaveEvent(user.Guild.Id));
            }
        }

        private async Task HandleUserBanned(UserBanned user)
        {
            var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
            if (!cfg.LogBan.HasValue) return;
            var queue = _tasks.BanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserBanned>());
            queue.Enqueue(user);
            if (UserBannedGuildTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = UserBannedGuildTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessUserBannedEvent,
                        (key, old) => _tasks.ProcessUserBannedEvent(user.Guild.Id));
            }
            else
            {
                UserBannedGuildTasks.TryAdd(user.Guild.Id, _tasks.ProcessUserBannedEvent(user.Guild.Id));
            }
        }

        private async Task HandleUserUnbanned(UserUnbanned user)
        {
            var cfg = await _db.GetOrCreateGuildConfig(user.Guild).ConfigureAwait(false);
            if (!cfg.LogBan.HasValue) return;
            var queue = _tasks.UnBanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserUnbanned>());
            queue.Enqueue(user);
            if (UserUnBannedGuildTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = UserUnBannedGuildTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessUserUnbannedEvent,
                        (key, old) => _tasks.ProcessUserUnbannedEvent(user.Guild.Id));
            }
            else
            {
                UserUnBannedGuildTasks.TryAdd(user.Guild.Id, _tasks.ProcessUserUnbannedEvent(user.Guild.Id));
            }
        }

        private async Task HandleMessageDeleted(MessageDeleted message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return;
            var cfg = await _db.GetOrCreateGuildConfig(chx.Guild);
            if (!cfg.LogMsg.HasValue) return;
            var queue = _tasks.MessageDeletedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageDeleted>());
            queue.Enqueue(message);
            if (MessageDeletedTasks.TryGetValue(chx.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = MessageDeletedTasks.AddOrUpdate(chx.Guild.Id, _tasks.ProcessMessageDeletedEvent,
                        (key, old) => _tasks.ProcessMessageDeletedEvent(chx.Guild.Id));
            }
            else
            {
                MessageDeletedTasks.TryAdd(chx.Guild.Id, _tasks.ProcessMessageDeletedEvent(chx.Guild.Id));
            }
        }

        private async Task HandleMessageUpdated(MessageUpdated message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return;
            var cfg = await _db.GetOrCreateGuildConfig(chx.Guild);
            if (!cfg.LogMsg.HasValue) return;
            var queue = _tasks.MessageUpdatedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageUpdated>());
            queue.Enqueue(message);
            if (MessageUpdatedTasks.TryGetValue(chx.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = MessageUpdatedTasks.AddOrUpdate(chx.Guild.Id, _tasks.ProcessMessageUpdatedEvent,
                        (key, old) => _tasks.ProcessMessageUpdatedEvent(chx.Guild.Id));
            }
            else
            {
                MessageUpdatedTasks.TryAdd(chx.Guild.Id, _tasks.ProcessMessageUpdatedEvent(chx.Guild.Id));
            }
        }

        private async Task HandleUserUpdated(UserUpdated user)
        {
            if (!(user.NewUser is SocketGuildUser gusr)) return;
            var cfg = await _db.GetOrCreateGuildConfig(gusr.Guild);
            if (!cfg.LogAvi.HasValue) return;
            var queue = _tasks.UserUpdatedQueue.GetOrAdd(gusr.Guild.Id, new ConcurrentQueue<UserUpdated>());
            queue.Enqueue(user);
            if (UserUpdatedTasks.TryGetValue(gusr.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = UserUpdatedTasks.AddOrUpdate(gusr.Guild.Id, _tasks.ProcessUserUpdatedEvent,
                        (key, old) => _tasks.ProcessUserUpdatedEvent(gusr.Guild.Id));
            }
            else
            {
                UserUpdatedTasks.TryAdd(gusr.Guild.Id, _tasks.ProcessUserUpdatedEvent(gusr.Guild.Id));
            }
        }

        private async Task HandleGuildUserUpdated(GuildUserUpdated user)
        {
            var usr = user.NewUser;
            var cfg = await _db.GetOrCreateGuildConfig(usr.Guild);
            if (!cfg.LogAvi.HasValue) return;
            var queue = _tasks.GuildMemberUpdatedQueue.GetOrAdd(usr.Guild.Id, new ConcurrentQueue<GuildUserUpdated>());
            queue.Enqueue(user);
            if (GuildMemberUpdatedTasks.TryGetValue(usr.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                    _ = GuildMemberUpdatedTasks.AddOrUpdate(usr.Guild.Id, _tasks.ProcessGuildUserUpdatedEvent,
                        (key, old) => _tasks.ProcessGuildUserUpdatedEvent(usr.Guild.Id));
            }
            else
            {
                GuildMemberUpdatedTasks.TryAdd(usr.Guild.Id, _tasks.ProcessGuildUserUpdatedEvent(usr.Guild.Id));
            }
        }
    }
}