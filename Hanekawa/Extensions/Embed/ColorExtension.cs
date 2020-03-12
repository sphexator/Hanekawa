using System;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Extensions.Embed
{
    public static class ColorExtension
    {
        /*
        private static readonly MemoryCache Config = new MemoryCache(new MemoryCacheOptions());

        public static void UpdateConfig(ulong guildId, GuildConfig cfg)
            => Config.Set(guildId, cfg, TimeSpan.FromHours(1));

        public static Color GetDefaultColor(this Color color, ulong guildId)
        {
            GuildConfig cfg;
            var check = Config.TryGetValue(guildId, out var result);
            if (!check) cfg = GetAndUpdateConfig(guildId);
            else cfg = (GuildConfig) result;
            return new Color(cfg.EmbedColor);
        }

        private static GuildConfig GetAndUpdateConfig(ulong guild, IServiceProvider provider)
        {
            using (var db = provider.GetRequiredService<DbService>())
            {
                var cfg = db.GetOrCreateGuildConfig(guild);
                Config.Set(guild, cfg, TimeSpan.FromMinutes(10));
                return cfg;
            }
        }
        */
    }
}