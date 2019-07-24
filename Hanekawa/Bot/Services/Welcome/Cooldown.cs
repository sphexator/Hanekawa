using System;
using System.Collections.Concurrent;
using Discord.WebSocket;
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

        private bool OnCooldown(SocketGuildUser user)
        {
            var users = _cooldown.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return true;
            users.Set(user.Id, user, TimeSpan.FromSeconds(60));
            return false;
        }

        private bool IsRatelimited(SocketGuildUser user, WelcomeConfig cfg)
        {
            var users = _ratelimit.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.Count + 1 >= cfg.Limit) return true;
            users.Set(user.Id, user.Id, TimeSpan.FromSeconds(5));
            return false;
        }
    }
}