using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class AutoModerator
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime time { get; set; }
    }
}
