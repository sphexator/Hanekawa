using System;

namespace Hanekawa.Entities.Moderation
{
    public class MuteTimer
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}