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
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<ExpSource, double>> ExperienceMultipliers = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<EmoteType, IEmoji>> Emote = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<string, Tuple<Snowflake, int>>> GuildInvites = new();
        public readonly ConcurrentDictionary<Snowflake, ConcurrentDictionary<Snowflake, Timer>> MuteTimers = new();
        public readonly ConcurrentDictionary<Snowflake, HashSet<IPrefix>> GuildPrefix = new();
        public readonly ConcurrentDictionary<Snowflake, Color> GuildEmbedColors = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> Board = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> BanCache = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> QuoteCache = new();
        public readonly ConcurrentDictionary<Snowflake, MemoryCache> Drops = new();
        
        public Color GetColor(IGuild guild) => GetColor(guild.Id);
        public Color GetColor(Snowflake guildSnowflake) => GuildEmbedColors.TryGetValue(guildSnowflake, out var color)
            ? color
            : HanaBaseColor.Default();
        
        public HashSet<IPrefix> GetPrefix(IGuild guild) => GetPrefix(guild.Id);
        public HashSet<IPrefix> GetPrefix(Snowflake guildSnowflake) => GuildPrefix.TryGetValue(guildSnowflake, out var prefix)
            ? prefix
            : null;
        
        public void AdjustExpMultiplier(ExpSource type, Snowflake guildId, double multiplier)
            => ExperienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<ExpSource, double>())
                .AddOrUpdate(type, multiplier, (_, _) => multiplier);
        public bool TryGetMultiplier(ExpSource type, Snowflake guildId, out double multiplier)
            => ExperienceMultipliers.GetOrAdd(guildId, new ConcurrentDictionary<ExpSource, double>())
                .TryGetValue(type, out multiplier);
        public bool TryGetEmote(EmoteType type, Snowflake guildId, out IEmoji emote)
            => Emote.GetOrAdd(guildId, new ConcurrentDictionary<EmoteType, IEmoji>()).TryGetValue(type, out emote);
        
        public void Dispose(Snowflake guildId)
        {
            Cooldown.TryRemove(guildId, out _);
            GuildInvites.TryRemove(guildId, out _);
            MuteTimers.TryRemove(guildId, out _);
            ExperienceMultipliers.TryRemove(guildId, out _);
            GuildPrefix.TryRemove(guildId, out _);
            BanCache.TryRemove(guildId, out _);
            QuoteCache.TryRemove(guildId, out _);
            GuildEmbedColors.TryRemove(guildId, out _);
            Emote.TryRemove(guildId, out _);
        }
    }
}