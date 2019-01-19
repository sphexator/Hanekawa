using System;

namespace Hanekawa.Addons.Database.Tables.Moderation
{
    public class MuteTimer
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Time { get; set; }
    }
}