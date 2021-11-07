using System;

namespace Hanekawa.Entities.Premium
{
    public class MvpConfig
    {
        public ulong GuildId { get; set; }
        public bool Disabled { get; set; }
        public DayOfWeek Day { get; set; } = DayOfWeek.Wednesday;
        public ulong? RoleId { get; set; } = null;
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
        public int Count { get; set; } = 5;
    }
}