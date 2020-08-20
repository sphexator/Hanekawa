using System;

namespace Hanekawa.Database.Tables.Premium
{
    public class MvpConfig
    {
        public ulong GuildId { get; set; }
        public DayOfWeek Day { get; set; } = DayOfWeek.Wednesday;
        public ulong? RoleId { get; set; } = null;
        public int Count { get; set; } = 5;
    }
}