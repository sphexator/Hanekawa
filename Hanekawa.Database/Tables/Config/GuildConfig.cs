using System;
using System.Collections.Generic;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Database.Tables.Config
{
    public class GuildConfig
    {
        public Snowflake GuildId { get; set; }
        public List<string> Prefix { get; set; } = new List<string> {"h."};
        public DateTimeOffset? Premium { get; set; } = null;
        public int EmbedColor { get; set; }

        // Premium
        public Snowflake? MvpChannel { get; set; } = null;
        
        public AdminConfig AdminConfig { get; set; }
        public Guild.BoardConfig BoardConfig { get; set; }
        public BoostConfig BoostConfig { get; set; }
        public ChannelConfig ChannelConfig { get; set; }
        public ClubConfig ClubConfig { get; set; }
        public CurrencyConfig CurrencyConfig { get; set; }
        public DropConfig DropConfig { get; set; }
        public LevelConfig LevelConfig { get; set; }
        public LoggingConfig LoggingConfig { get; set; }
        public SuggestionConfig SuggestionConfig { get; set; }
        public WelcomeConfig WelcomeConfig { get; set; }
    }
}