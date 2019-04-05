using System;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config;
using Microsoft.Extensions.Caching.Memory;

namespace Hanekawa.Extensions.Embed
{
    public static class ColorExtension
    {
        private static readonly MemoryCache Config = new MemoryCache(new MemoryCacheOptions());

        public static void UpdateConfig(ulong guildId, GuildConfig cfg) 
            => Config.Set(guildId, cfg, TimeSpan.FromHours(1));

        public static Color GetDefaultColor(this Color color, ulong guildId, DbService db)
        {
            GuildConfig cfg;
            var check = Config.TryGetValue(guildId, out var result);
            if (!check && db == null) return Color.Purple;
            if (!check) cfg = GetAndUpdateConfig(guildId, db);
            else cfg = (GuildConfig)result;
            return new Color(cfg.EmbedColor);
        }

        private static GuildConfig GetAndUpdateConfig(ulong guild, DbService db)
        {
            var cfg = db.GetOrCreateGuildConfig(guild);
            Config.Set(guild, cfg, TimeSpan.FromMinutes(10));
            return cfg;
        }
    }
}