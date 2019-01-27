using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Services.Drop
{
    public class DropData : IHanaService
    {
        private readonly MemoryCache _guildCooldown = new MemoryCache(new MemoryCacheOptions());

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _lootChannels
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _normalLoot =
            new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>> _spawnedLoot =
            new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, bool>>();

        private readonly ConcurrentDictionary<ulong, MemoryCache> _userCooldown =
            new ConcurrentDictionary<ulong, MemoryCache>();

        public DropData()
        {
            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var result = db.LootChannels.Where(c => c.GuildId == x.GuildId).ToList();
                    if (result.Count == 0) continue;
                    var channelList = _lootChannels.GetOrAdd(x.GuildId, new ConcurrentDictionary<ulong, bool>());
                    foreach (var y in result) channelList.GetOrAdd(y.ChannelId, true);
                }
            }
        }

        public bool IsLootMessage(ulong guildId, ulong messageId, out bool special)
        {
            var regular = _normalLoot.GetOrAdd(guildId, new ConcurrentDictionary<ulong, bool>());
            var spawned = _spawnedLoot.GetOrAdd(guildId, new ConcurrentDictionary<ulong, bool>());
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

        public bool IsLootChannel(ulong guildId, ulong channelId)
        {
            var regular = _lootChannels.GetOrAdd(guildId, new ConcurrentDictionary<ulong, bool>());
            return regular.TryGetValue(channelId, out _);
        }

        public bool OnGuildCooldown(ITextChannel guild)
        {
            if (_guildCooldown.TryGetValue(guild.Id, out _)) return false;
            _guildCooldown.Set(guild.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
            return true;
        }

        public bool OnUserCooldown(IGuildUser user)
        {
            var users = _userCooldown.GetOrAdd(user.GuildId, new MemoryCache(new MemoryCacheOptions()));
            if (users.TryGetValue(user.Id, out _)) return false;
            users.Set(user.Id, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            return true;
        }

        public void AddRegular(IGuild guild, IMessage message)
        {
            var normal = _normalLoot.GetOrAdd(guild.Id, new ConcurrentDictionary<ulong, bool>());
            normal.TryAdd(message.Id, true);
        }

        public void AddSpecial(IGuild guild, IMessage message)
        {
            var normal = _normalLoot.GetOrAdd(guild.Id, new ConcurrentDictionary<ulong, bool>());
            normal.TryAdd(message.Id, true);
        }

        public void RemoveRegular(IGuild guild, IMessage message)
        {
            var normal = _normalLoot.GetOrAdd(guild.Id, new ConcurrentDictionary<ulong, bool>());
            normal.TryRemove(message.Id, out _);
        }

        public void RemoveSpecial(IGuild guild, IMessage message)
        {
            var normal = _normalLoot.GetOrAdd(guild.Id, new ConcurrentDictionary<ulong, bool>());
            normal.TryRemove(message.Id, out _);
        }

        public async Task AddLootChannelAsync(SocketTextChannel ch)
        {
            await AddToDatabaseAsync(ch.Guild.Id, ch.Id);
            var channels = _lootChannels.GetOrAdd(ch.Guild.Id, new ConcurrentDictionary<ulong, bool>());
            channels.TryAdd(ch.Id, true);
            _lootChannels.AddOrUpdate(ch.Guild.Id, channels, (key, old) => old = channels);
        }

        public async Task RemoveLootChannelAsync(SocketTextChannel ch)
        {
            await RemoveFromDatabaseAsync(ch.Guild.Id, ch.Id);
            if (!_lootChannels.TryGetValue(ch.Guild.Id, out var channels)) return;
            if (!channels.TryGetValue(ch.Id, out _)) return;
            channels.TryRemove(ch.Id, out _);
            _lootChannels.AddOrUpdate(ch.Guild.Id, channels, (key, old) => channels);
        }

        private static async Task AddToDatabaseAsync(ulong guildId, ulong channelId)
        {
            using (var db = new DbService())
            {
                var data = new LootChannel
                {
                    ChannelId = channelId,
                    GuildId = guildId
                };
                await db.LootChannels.AddAsync(data);
                await db.SaveChangesAsync();
            }
        }

        private static async Task RemoveFromDatabaseAsync(ulong guildId, ulong channelId)
        {
            using (var db = new DbService())
            {
                var data = db.LootChannels.First(x => x.GuildId == guildId && x.ChannelId == channelId);
                db.LootChannels.Remove(data);
                await db.SaveChangesAsync();
            }
        }
    }
}