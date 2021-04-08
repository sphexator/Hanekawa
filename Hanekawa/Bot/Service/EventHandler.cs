using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Administration;
using Hanekawa.Bot.Service.Administration.Mute;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Logs;
using Hanekawa.Entities;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace Hanekawa.Bot.Service
{
    public class EventHandler : INService, IJob
    {
        private readonly Hanekawa _client;
        private readonly CacheService _cache;
        private readonly Experience _experience;
        private readonly LogService _logService;
        private readonly BlacklistService _blacklist;
        private readonly MuteService _mute;

        public EventHandler(Hanekawa client, Experience experience, CacheService cache, LogService logService, BlacklistService blacklist, MuteService mute)
        {
            _client = client;
            _experience = experience;
            _cache = cache;
            _logService = logService;
            _blacklist = blacklist;
            _mute = mute;

            _client.MessageReceived += MessageReceived;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageDeleted += MessageDeleted;
            _client.MessagesDeleted += MessagesDeleted;

            _client.MemberJoined += MemberJoined;
            _client.MemberLeft += MemberLeft;
            _client.MemberUpdated += MemberUpdated;

            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
            _client.ReactionsCleared += ReactionsCleared;
            
            _client.VoiceStateUpdated += VoiceStateUpdated;
            
            _client.BanCreated += BanCreated;
            _client.BanDeleted += BanDeleted;
            
            _client.RoleDeleted += RoleDeleted;

            _client.ChannelDeleted += ChannelDeleted;

            _client.JoinedGuild += JoinedGuild;
            _client.LeftGuild += LeftGuild;
        }

        private Task MessagesDeleted(object sender, MessagesDeletedEventArgs e)
        {
            _ = _logService.MessagesDeletedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            _ = _logService.MessageDeletedAsync(e);
            return Task.CompletedTask;
        }

        private Task MessageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            _ = _logService.MessageUpdatedAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberJoined(object sender, MemberJoinedEventArgs e)
        {
            _ = _logService.JoinLogAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberLeft(object sender, MemberLeftEventArgs e)
        {
            _ = _logService.LeaveLogAsync(e);
            return Task.CompletedTask;
        }

        private Task MemberUpdated(object sender, MemberUpdatedEventArgs e)
        {
            _ = _logService.MemberUpdatedAsync(e);
            return Task.CompletedTask;
        }

        private Task JoinedGuild(object sender, JoinedGuildEventArgs e)
        {
            _ = _blacklist.BlackListAsync(e);
            return Task.CompletedTask;
        }

        private Task LeftGuild(object sender, LeftGuildEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task ChannelDeleted(object sender, ChannelDeletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task RoleDeleted(object sender, RoleDeletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task BanDeleted(object sender, BanDeletedEventArgs e)
        {
            _ = _logService.UnbanAsync(e);
            return Task.CompletedTask;
        }

        private Task BanCreated(object sender, BanCreatedEventArgs e)
        {
            _ = _logService.BanAsync(e);
            return Task.CompletedTask;
        }

        private Task VoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task ReactionsCleared(object sender, ReactionsClearedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task ReactionRemoved(object sender, ReactionRemovedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task ReactionAdded(object sender, ReactionAddedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return Task.CompletedTask;
            var guildCache = _cache.Cooldown
                .GetOrAdd(e.GuildId.Value, new ConcurrentDictionary<CooldownType, MemoryCache>());
            var msgCache = guildCache.GetOrAdd(CooldownType.ServerMessage, new MemoryCache(new MemoryCacheOptions()));
            var cdCheck = msgCache.TryGetValue(e.Member.Id.RawValue, out _);
            
            if (!cdCheck) msgCache.Set(e.Member.Id.RawValue, 0, TimeSpan.FromMinutes(1));

            guildCache.AddOrUpdate(CooldownType.ServerMessage, msgCache, (_, _) => msgCache);
            if(!cdCheck) _ = _experience.ServerExperienceAsync(e);
            if(!cdCheck) _ = _experience.GlobalExperienceAsync(e);
            return Task.CompletedTask;
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
