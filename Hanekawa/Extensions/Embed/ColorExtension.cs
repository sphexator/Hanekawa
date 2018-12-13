using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using System;
using System.Collections.Concurrent;

namespace Hanekawa.Extensions.Embed
{
    public static class ColorExtension
    {
        private static readonly ConcurrentDictionary<ulong, Tuple<GuildConfig, DateTime>> GuildConfig 
            = new ConcurrentDictionary<ulong, Tuple<GuildConfig, DateTime>>();

        public static Color GetDefaultColor(this Color color, ulong guild)
        {
            GuildConfig cfg;
            var check = GuildConfig.TryGetValue(guild, out var result);
            if (!check) cfg = GetConfig(guild);
            else if (result.Item2.AddMinutes(10) <= DateTime.UtcNow) cfg = UpdateConfig(guild);
            else cfg = result.Item1;
            return cfg == null ? Color.Purple : new Color(cfg.EmbedColor);
        }

        private static GuildConfig GetConfig(ulong guild)
        {
            GuildConfig cfg;
            using (var db = new DbService())
            {
                cfg = db.GetOrCreateGuildConfig(guild);
                var toAdd = new Tuple<GuildConfig, DateTime>(cfg, DateTime.UtcNow);
                GuildConfig.TryAdd(guild, toAdd);
            }

            return cfg;
        }

        private static GuildConfig UpdateConfig(ulong guild)
        {
            GuildConfig cfg;
            using (var db = new DbService())
            {
                cfg = db.GetOrCreateGuildConfig(guild);
                var toAdd = new Tuple<GuildConfig, DateTime>(cfg, DateTime.UtcNow);
                GuildConfig.AddOrUpdate(guild, toAdd, (key, old) => toAdd);
            }

            return cfg;
        }
    }
}
