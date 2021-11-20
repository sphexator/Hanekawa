using System;
using Disqord;

namespace Hanekawa.Database.Tables.Premium
{
    public class MvpConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelId { get; set; } = null;
        public bool Disabled { get; set; }
        public DayOfWeek Day { get; set; } = DayOfWeek.Wednesday;
        public Snowflake? RoleId { get; set; } = null;
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
        public int Count { get; set; } = 5;
    }
}