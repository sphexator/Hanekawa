using System;
using System.Collections.Concurrent;
using Disqord;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private readonly MemoryCache _globalCooldown = new MemoryCache(new MemoryCacheOptions());

        private readonly ConcurrentDictionary<ulong, MemoryCache> _serverExpCooldown
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private bool OnServerCooldown(CachedMember user)
        {
            var users = _serverExpCooldown.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id.RawValue, out _)) return true;
            users.Set(user.Id.RawValue, user, TimeSpan.FromSeconds(60));
            return false;
        }

        private bool OnGlobalCooldown(CachedMember user)
        {
            if (_globalCooldown.TryGetValue(user.Id.RawValue, out _)) return true;
            _globalCooldown.Set(user.Id.RawValue, user, TimeSpan.FromMinutes(1));
            return false;
        }
    }
}