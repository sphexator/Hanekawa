using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Disqord;
using Disqord.Bot;
using Hanekawa.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Bot.Service.Cache
{
    public class CacheService : INService
    {
        public readonly MemoryCache GlobalCooldown = new(new MemoryCacheOptions());
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<CooldownType, MemoryCache>> Cooldown = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<string, Tuple<Snowflake, int>>> GuildInvites = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Timer>> MuteTimers = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Database.Entities.ChannelType, double>> ExperienceMultipliers = new();
        public readonly ConcurrentDictionary<Snowflake, HashSet<IPrefix>> GuildPrefix = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> BanCache = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> QuoteCache = new();
        public readonly ConcurrentDictionary<Snowflake, Color> GuildEmbedColors = new();

        public Color GetColor(IGuild guild) => GetColor(guild.Id);
        public Color GetColor(Snowflake guildSnowflake) => GuildEmbedColors.TryGetValue(guildSnowflake, out var color)
            ? color
            : HanaBaseColor.Default();

        public HashSet<IPrefix> GetPrefix(IGuild guild) => GetPrefix(guild.Id);
        public HashSet<IPrefix> GetPrefix(Snowflake guildSnowflake) => GuildPrefix.TryGetValue(guildSnowflake, out var prefix)
            ? prefix
            : null;

        public void AdjustExpMultiplier(Database.Entities.ChannelType type, Snowflake guildId, double multiplier)
            => ExperienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<Database.Entities.ChannelType, double>())
                .AddOrUpdate(type, multiplier, (_, _) => multiplier);
        public double GetMultiplier(Database.Entities.ChannelType type, Snowflake guildId)
            => ExperienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<Database.Entities.ChannelType, double>())
                .GetOrAdd(type, 1);
    }
}