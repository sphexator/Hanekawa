using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Services.Level
{
    public class LevelData : IHanaService
    {
        private readonly ConcurrentDictionary<ulong, MemoryCache> _serverExpCooldown
            = new ConcurrentDictionary<ulong, MemoryCache>();
        private readonly MemoryCache _globalExpCooldown = new MemoryCache(new MemoryCacheOptions());

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverCategoryReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();
        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverChannelReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

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

        public bool IsReducedExp(ITextChannel channel)
        {
            _serverChannelReduction.TryGetValue(channel.GuildId, out var channels);
            _serverCategoryReduction.TryGetValue(channel.GuildId, out var category);
            if(channel.CategoryId.HasValue)
                if (channels.Contains(channel.CategoryId.Value))
                    return true;
            if (channels.Contains(channel.Id)) return true;
            return false;
        }

        public async Task<EmbedBuilder> ReducedExpManager(ITextChannel channel, bool remove)
        {
            using (var db = new DbService())
            {
                var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
                if (!remove)
                {
                    if (!channels.Contains(channel.Id)) return await AddReducedExp(db, channel);
                    return new EmbedBuilder().CreateDefault($"{channel.Mention} is already added.", Color.Red.RawValue);
                }

                if (channels.Contains(channel.Id)) return await RemoveReducedExp(db, channel);
                return new EmbedBuilder().CreateDefault($"Couldn't find {channel.Mention}", Color.Red.RawValue);
            }
        }

        public async Task<EmbedBuilder> ReducedExpManager(ICategoryChannel category, bool remove)
        {
            using (var db = new DbService())
            {
                if (!remove) return await AddReducedExp(db, category);

                var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
                if (channels.Contains(category.Id)) return await RemoveReducedExp(db, category);
                return new EmbedBuilder().CreateDefault($"Couldn't find {category.Name}", Color.Red.RawValue);
            }
        }

        public async Task<List<string>> ReducedExpList(IGuild guild)
        {
            using (var db = new DbService())
            {
                var isChannel = _serverChannelReduction.TryGetValue(guild.Id, out var channels);
                var isCategory = _serverCategoryReduction.TryGetValue(guild.Id, out var categories);
                var result = new List<string>();
                if (channels.Count == 0 && categories.Count == 0)
                {
                    result.Add("No channels");
                    return result;
                }

                if (isChannel)
                {
                    foreach (var x in channels)
                    {
                        result.Add($"Channel: {(await guild.GetTextChannelAsync(x)).Name}");
                    }
                }

                if (isCategory)
                {
                    foreach (var x in categories)
                    {
                        result.Add($"Category: {(await guild.GetCategoriesAsync()).First(y => y.Id == x).Name}\n");
                    }
                }

                return result;
            }
        }

        private async Task<EmbedBuilder> AddReducedExp(DbService db, ITextChannel channel)
        {
            var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
            channels.Add(channel.Id);
            var data = new LevelExpReduction
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                Channel = true,
                Category = false
            };
            await db.LevelExpReductions.AddAsync(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Added {channel.Name} to reduced exp list", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> AddReducedExp(DbService db, ICategoryChannel category)
        {
            var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
            channels.Add(category.Id);
            var data = new LevelExpReduction
            {
                GuildId = category.GuildId,
                ChannelId = category.Id,
                Channel = false,
                Category = true
            };
            await db.LevelExpReductions.AddAsync(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Added {category.Name} to reduced exp list", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveReducedExp(DbService db, ITextChannel channel)
        {
            var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
            channels.Remove(channel.Id);
            var data = await db.LevelExpReductions.FindAsync(channel.GuildId, channel.Id);
            db.LevelExpReductions.Remove(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Removed {channel.Name} from reduced exp list",
                Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveReducedExp(DbService db, ICategoryChannel category)
        {
            var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
            channels.Remove(category.Id);
            var data = await db.LevelExpReductions.FindAsync(category.GuildId, category.Id);
            db.LevelExpReductions.Remove(data);
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault($"Removed {category.Name} from reduced exp list",
                Color.Green.RawValue);
        }
    }
}
