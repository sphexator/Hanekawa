using System;
using System.Collections.Concurrent;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private readonly MemoryCache _globalCooldown = new MemoryCache(new MemoryCacheOptions());
        private readonly ConcurrentDictionary<ulong, MemoryCache> _serverExpCooldown 
            = new ConcurrentDictionary<ulong, MemoryCache>();
        
        public bool OnServerCooldown(SocketGuildUser user)
        {
            var users = _serverExpCooldown.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return true;
            users.Set(user.Id, user, TimeSpan.FromSeconds(60));
            return false;
        }

        public bool OnGlobalCooldown(SocketGuildUser user)
        {
            if (_globalCooldown.TryGetValue(user.Id, out _)) return true;
            _globalCooldown.Set(user.Id, user, TimeSpan.FromMinutes(1));
            return false;
        }
    }
}