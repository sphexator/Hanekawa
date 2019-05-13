using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Config
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public List<string> Prefix { get; set; } = new List<string> { "h." };
        public bool Premium { get; set; } = false;
        public uint EmbedColor { get; set; }

        // Premium
        public ulong? AnimeAirChannel { get; set; } = null;
        public bool AutomaticEventSchedule { get; set; } = false;

        // Channel settings

        // Club settings

        // Welcome Settings

        // Logging settings

        // Leveling settings

        // Currency Settings

        // Admin settings

        //Music Settings
        public ulong? MusicChannel { get; set; } = null;
        public ulong? MusicVcChannel { get; set; } = null;
    }
}