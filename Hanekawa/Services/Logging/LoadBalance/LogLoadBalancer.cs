using Discord.WebSocket;
using Hanekawa.Entities.Log;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Hanekawa.Services.Logging.LoadBalance
{
    public class LogLoadBalancer
    {
        private readonly Tasks _tasks;

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

        public LogLoadBalancer(Tasks tasks)
        {
            _tasks = tasks;
        }

        public void Add(LogType type, object entity)
        {
            switch (type)
            {
                case LogType.GuildUserUpdated:
                    break;
                case LogType.UserUpdated:
                    break;
                case LogType.MessageDeleted:
                    break;
                case LogType.MessageUpdated:
                    break;
                case LogType.UserBanned:
                    break;
                case LogType.UserUnbanned:
                    break;
                case LogType.UserJoined:
                    break;
                case LogType.UserLeft:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private Task HandleUserJoined(UserJoined usr)
        {
            var user = usr.User;
            var queue = _tasks.UserJoinedQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserJoined>());
            queue.Enqueue(usr);
            var check = UserJoinedTasks.ContainsKey(user.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(user.Guild.Id, new Task(() => { }));
            return Task.CompletedTask;
        }

        private Task HandleUserLeft(UserLeft usr)
        {
            var user = usr.User;
            var queue = _tasks.UserLeftQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserLeft>());
            queue.Enqueue(usr);
            var check = UserJoinedTasks.ContainsKey(user.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(user.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleUserBanned(UserBanned user)
        {
            var queue = _tasks.BanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserBanned>());
            queue.Enqueue(user);
            var check = UserJoinedTasks.ContainsKey(user.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(user.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleUserUnbanned(UserUnbanned user)
        {
            var queue = _tasks.UnBanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserUnbanned>());
            queue.Enqueue(user);
            var check = UserJoinedTasks.ContainsKey(user.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(user.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleMessageDeleted(MessageDeleted message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return Task.CompletedTask;
            var queue = _tasks.MessageDeletedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageDeleted>());
            queue.Enqueue(message);
            var check = UserJoinedTasks.ContainsKey(chx.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(chx.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleMessageUpdated(MessageUpdated message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return Task.CompletedTask;
            var queue = _tasks.MessageUpdatedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageUpdated>());
            queue.Enqueue(message);
            var check = UserJoinedTasks.ContainsKey(chx.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(chx.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleUserUpdated(UserUpdated user)
        {
            if (!(user.NewUser is SocketGuildUser gusr)) return Task.CompletedTask;
            var queue = _tasks.UserUpdatedQueue.GetOrAdd(gusr.Guild.Id, new ConcurrentQueue<UserUpdated>());
            queue.Enqueue(user);
            var check = UserJoinedTasks.ContainsKey(gusr.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(gusr.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }

        private Task HandleGuildUserUpdated(GuildUserUpdated user)
        {
            var usr = user.NewUser;
            var queue = _tasks.GuildMemberUpdatedQueue.GetOrAdd(usr.Guild.Id, new ConcurrentQueue<GuildUserUpdated>());
            queue.Enqueue(user);
            var check = UserJoinedTasks.ContainsKey(usr.Guild.Id);
            if (check) return Task.CompletedTask;
            UserJoinedTasks.TryAdd(usr.Guild.Id, new Task(() => { }));

            return Task.CompletedTask;
        }
    }
}
