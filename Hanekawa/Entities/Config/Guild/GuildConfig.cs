using System;

namespace Hanekawa.Entities.Config.Guild
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = "h.";
        public DateTimeOffset? Premium { get; set; } = null;
        public int EmbedColor { get; set; }
    }
}