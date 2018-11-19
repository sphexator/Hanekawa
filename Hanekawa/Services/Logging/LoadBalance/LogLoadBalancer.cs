﻿using Discord.WebSocket;
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
                    var gUserUpdated = (GuildUserUpdated) entity;
                    HandleGuildUserUpdated(gUserUpdated);
                    break;
                case LogType.UserUpdated:
                    var userUpdated = (UserUpdated) entity;
                    HandleUserUpdated(userUpdated);
                    break;
                case LogType.MessageDeleted:
                    var messageDeleted = (MessageDeleted)entity;
                    HandleMessageDeleted(messageDeleted);
                    break;
                case LogType.MessageUpdated:
                    var messageUpdated = (MessageUpdated)entity;
                    HandleMessageUpdated(messageUpdated);
                    break;
                case LogType.UserBanned:
                    var userBanned = (UserBanned)entity;
                    HandleUserBanned(userBanned);
                    break;
                case LogType.UserUnbanned:
                    var userUnbanned = (UserUnbanned)entity;
                    HandleUserUnbanned(userUnbanned);
                    break;
                case LogType.UserJoined:
                    var userJoined = (UserJoined)entity;
                    HandleUserJoined(userJoined);
                    break;
                case LogType.UserLeft:
                    var userLeft = (UserLeft)entity;
                    HandleUserLeft(userLeft);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void HandleUserJoined(UserJoined usr)
        {
            var user = usr.User;
            var queue = _tasks.UserJoinedQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserJoined>());
            queue.Enqueue(usr);
            if (UserJoinedTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessJoinEvent, (key, old) => _tasks.ProcessJoinEvent(user.Guild.Id));
                }
            }
            else
            {
                UserJoinedTasks.TryAdd(user.Guild.Id, _tasks.ProcessJoinEvent(user.Guild.Id)); ;
            }
        }

        private void HandleUserLeft(UserLeft usr)
        {
            var user = usr.User;
            var queue = _tasks.UserLeftQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserLeft>());
            queue.Enqueue(usr);
            if (UserLeftTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessLeaveEvent, (key, old) => _tasks.ProcessLeaveEvent(user.Guild.Id));
                }
            }
            else
            {
                UserLeftTasks.TryAdd(user.Guild.Id, _tasks.ProcessLeaveEvent(user.Guild.Id)); ;
            }
        }

        private void HandleUserBanned(UserBanned user)
        {
            var queue = _tasks.BanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserBanned>());
            queue.Enqueue(user);
            if (UserBannedGuildTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessUserBannedEvent, (key, old) => _tasks.ProcessUserBannedEvent(user.Guild.Id));
                }
            }
            else
            {
                UserBannedGuildTasks.TryAdd(user.Guild.Id, _tasks.ProcessUserBannedEvent(user.Guild.Id)); ;
            }
        }

        private void HandleUserUnbanned(UserUnbanned user)
        {
            var queue = _tasks.UnBanQueue.GetOrAdd(user.Guild.Id, new ConcurrentQueue<UserUnbanned>());
            queue.Enqueue(user);
            if (UserUnBannedGuildTasks.TryGetValue(user.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(user.Guild.Id, _tasks.ProcessUserUnbannedEvent, (key, old) => _tasks.ProcessUserUnbannedEvent(user.Guild.Id));
                }
            }
            else
            {
                UserUnBannedGuildTasks.TryAdd(user.Guild.Id, _tasks.ProcessUserUnbannedEvent(user.Guild.Id)); ;
            }
        }

        private void HandleMessageDeleted(MessageDeleted message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return;
            var queue = _tasks.MessageDeletedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageDeleted>());
            queue.Enqueue(message);
            if (MessageDeletedTasks.TryGetValue(chx.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(chx.Guild.Id, _tasks.ProcessMessageDeletedEvent, (key, old) => _tasks.ProcessMessageDeletedEvent(chx.Guild.Id));
                }
            }
            else
            {
                MessageDeletedTasks.TryAdd(chx.Guild.Id, _tasks.ProcessMessageDeletedEvent(chx.Guild.Id)); ;
            }
        }

        private void HandleMessageUpdated(MessageUpdated message)
        {
            if (!(message.Channel is SocketGuildChannel chx)) return;
            var queue = _tasks.MessageUpdatedQueue.GetOrAdd(chx.Guild.Id, new ConcurrentQueue<MessageUpdated>());
            queue.Enqueue(message);
            if (MessageUpdatedTasks.TryGetValue(chx.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(chx.Guild.Id, _tasks.ProcessMessageUpdatedEvent, (key, old) => _tasks.ProcessMessageUpdatedEvent(chx.Guild.Id));
                }
            }
            else
            {
                MessageUpdatedTasks.TryAdd(chx.Guild.Id, _tasks.ProcessMessageUpdatedEvent(chx.Guild.Id)); ;
            }
        }

        private void HandleUserUpdated(UserUpdated user)
        {
            if (!(user.NewUser is SocketGuildUser gusr)) return;
            var queue = _tasks.UserUpdatedQueue.GetOrAdd(gusr.Guild.Id, new ConcurrentQueue<UserUpdated>());
            queue.Enqueue(user);
            if (UserUpdatedTasks.TryGetValue(gusr.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(gusr.Guild.Id, _tasks.ProcessUserUpdatedEvent, (key, old) => _tasks.ProcessUserUpdatedEvent(gusr.Guild.Id));
                }
            }
            else
            {
                UserUpdatedTasks.TryAdd(gusr.Guild.Id, _tasks.ProcessUserUpdatedEvent(gusr.Guild.Id)); ;
            }
        }

        private void HandleGuildUserUpdated(GuildUserUpdated user)
        {
            var usr = user.NewUser;
            var queue = _tasks.GuildMemberUpdatedQueue.GetOrAdd(usr.Guild.Id, new ConcurrentQueue<GuildUserUpdated>());
            queue.Enqueue(user);
            if (GuildMemberUpdatedTasks.TryGetValue(usr.Guild.Id, out var task))
            {
                if (task.IsCompleted)
                {
                    GuildMemberUpdatedTasks.AddOrUpdate(usr.Guild.Id, _tasks.ProcessGuildUserUpdatedEvent, (key, old) => _tasks.ProcessGuildUserUpdatedEvent(usr.Guild.Id));
                }
            }
            else
            {
                GuildMemberUpdatedTasks.TryAdd(usr.Guild.Id, _tasks.ProcessGuildUserUpdatedEvent(usr.Guild.Id)); ;
            }
        }
    }
}
