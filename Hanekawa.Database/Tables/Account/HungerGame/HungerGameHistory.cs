using System;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameHistory
    {
        public Guid GameId { get; set; }
        public ulong GuildId { get; set; }
        public ulong Winner { get; set; }
        public DateTimeOffset Date { get; set; }
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
    }
}