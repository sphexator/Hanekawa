using System;
using System.Collections.Generic;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Database.Tables.Config
{
    public class GuildConfig
    {
        public Snowflake GuildId { get; set; }
        public string Prefix { get; set; } = "h.";
        public DateTimeOffset? Premium { get; set; } = null;
        public int EmbedColor { get; set; }

        // Premium
        public Snowflake? MvpChannel { get; set; } = null;
    }
}