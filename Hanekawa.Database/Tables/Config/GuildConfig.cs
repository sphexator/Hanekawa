using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Config
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = "h.";
        public bool Premium { get; set; } = false;
        public uint EmbedColor { get; set; }

        // Premium
        public ulong? AnimeAirChannel { get; set; } = null;
        public bool AutomaticEventSchedule { get; set; } = false;
        public ulong? MvpChannel { get; set; } = null;

        //Music Settings
        public ulong? MusicChannel { get; set; } = null;
        public ulong? MusicVcChannel { get; set; } = null;
    }
}