using System;
using System.Collections.Concurrent;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Welcome
{
    public partial class WelcomeService
    {
        private readonly ConcurrentDictionary<ulong, MemoryCache> _cooldown
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _ratelimit
            = new ConcurrentDictionary<ulong, MemoryCache>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _rewardCd =
            new ConcurrentDictionary<ulong, MemoryCache>();

        private bool OnCooldown(CachedMember user)
        {
            var users = _cooldown.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id.RawValue, out _)) return true;
            users.Set(user.Id.RawValue, 0, TimeSpan.FromSeconds(60));
            return false;
        }

        private bool IsRatelimited(CachedMember user, WelcomeConfig cfg)
        {
            var users = _ratelimit.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            if (users.Count + 1 >= cfg.Limit) return true;
            users.Set(user.Id.RawValue, 0, TimeSpan.FromSeconds(5));
            return false;
        }

        private bool IsRewardCd(CachedMember user)
        {
            var users = _rewardCd.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id.RawValue, out _)) return true;
            users.Set(user.Id.RawValue, 0, TimeSpan.FromMinutes(1));
            return false;
        }
    }
}