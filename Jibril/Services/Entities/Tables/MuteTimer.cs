using System;

namespace Jibril.Services.Entities.Tables
{
    public class MuteTimer
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Time { get; set; }
    }
}
