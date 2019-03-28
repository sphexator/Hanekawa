using System;
using System.Collections.Concurrent;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private readonly MemoryCache _guildCooldown = new MemoryCache(new MemoryCacheOptions());
        
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _lootChannels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _normalLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _spawnedLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();
        
        private readonly ConcurrentDictionary<ulong, MemoryCache> _userCooldown =
            new ConcurrentDictionary<ulong, MemoryCache>();

        
        private bool IsDropChannel(SocketTextChannel channel)
        {
            var channels = _lootChannels.GetOrAdd(channel.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            return channels.TryGetValue(channel.Id, out _);
        }
        
        private bool IsDropMessage(ulong guildId, ulong messageId, out bool special)
        {
            var regular = _normalLoot.GetOrAdd(guildId, new MemoryCache(new MemoryCacheOptions()));
            var spawned = _spawnedLoot.GetOrAdd(guildId, new MemoryCache(new MemoryCacheOptions()));
            if (regular.TryGetValue(messageId, out _))
            {
                special = false;
                return true;
            }

            if (spawned.TryGetValue(messageId, out _))
            {
                special = true;
                return true;
            }

            special = false;
            return false;
        }
        
        private bool OnUserCooldown(SocketGuildUser user)
        {
            var users = _userCooldown.GetOrAdd(user.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return true;
            users.Set(user.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return false;
        }

        private bool OnGuildCooldown(SocketGuild guild)
        {
            if (_guildCooldown.TryGetValue(guild.Id, out _)) return true;
            _guildCooldown.Set(guild.Id, true, TimeSpan.FromMinutes(1));
            return false;
        }
    }
}