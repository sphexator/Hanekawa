using System;
using System.Collections.Concurrent;
using Discord;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Services.Level
{
    public class Cooldown : IHanaService
    {
        private readonly MemoryCache _globalExpCooldown = new MemoryCache(new MemoryCacheOptions());

        private readonly ConcurrentDictionary<ulong, MemoryCache> _serverExpCooldown
            = new ConcurrentDictionary<ulong, MemoryCache>();

        public bool ServerCooldown(IGuildUser user)
        {
            var users = _serverExpCooldown.GetOrAdd(user.GuildId, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return false;
            users.Set(user.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return true;
        }

        public bool GlobalCooldown(IGuildUser user)
        {
            if (_globalExpCooldown.TryGetValue(user.Id, out _)) return false;
            _globalExpCooldown.Set(user.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return true;
        }
    }
}