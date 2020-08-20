using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private static readonly MemoryCache _guildCooldown = new MemoryCache(new MemoryCacheOptions());

        private static readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _lootChannels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private static readonly ConcurrentDictionary<ulong, MemoryCache> _normalLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();

        private static readonly ConcurrentDictionary<ulong, MemoryCache> _spawnedLoot =
            new ConcurrentDictionary<ulong, MemoryCache>();

        private static readonly ConcurrentDictionary<ulong, MemoryCache> _userCooldown =
            new ConcurrentDictionary<ulong, MemoryCache>();

        public async Task<bool> AddLootChannel(CachedTextChannel channel, DbService db)
        {
            var channels = _lootChannels.GetOrAdd(channel.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
            if (channels.ContainsKey(channel.Id.RawValue)) return false;
            channels.TryAdd(channel.Id.RawValue, true);
            var data = new LootChannel
            {
                GuildId = channel.Guild.Id.RawValue,
                ChannelId = channel.Id.RawValue
            };
            await db.LootChannels.AddAsync(data);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveLootChannel(CachedTextChannel channel, DbService db)
        {
            var channels = _lootChannels.GetOrAdd(channel.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
            if (!channels.ContainsKey(channel.Id.RawValue)) return false;
            channels.TryRemove(channel.Id.RawValue, out _);
            var data = await db.LootChannels.FirstOrDefaultAsync(x =>
                x.GuildId == channel.Guild.Id.RawValue && x.ChannelId == channel.Id.RawValue);
            if (data != null)
            {
                db.LootChannels.Remove(data);
                await db.SaveChangesAsync();
            }

            return true;
        }

        private bool IsDropChannel(CachedTextChannel channel)
        {
            var channels = _lootChannels.GetOrAdd(channel.Guild.Id.RawValue, new ConcurrentDictionary<ulong, bool>());
            return channels.TryGetValue(channel.Id.RawValue, out _);
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

        private bool OnUserCooldown(CachedMember user)
        {
            var users = _userCooldown.GetOrAdd(user.Guild.Id.RawValue, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id.RawValue, out _)) return true;
            users.Set(user.Id.RawValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return false;
        }

        private bool OnGuildCooldown(CachedGuild guild)
        {
            if (_guildCooldown.TryGetValue(guild.Id.RawValue, out _)) return true;
            _guildCooldown.Set(guild.Id.RawValue, true, TimeSpan.FromMinutes(1));
            return false;
        }
    }
}