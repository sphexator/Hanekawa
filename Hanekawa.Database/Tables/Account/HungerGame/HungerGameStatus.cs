using System;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameStatus
    {
        public ulong GuildId { get; set; }
        public ulong? SignUpChannel { get; set; } = null;
        public ulong? EventChannel { get; set; } = null;
        public string EmoteMessageFormat { get; set; } 

        public HungerGameStage Stage { get; set; } = HungerGameStage.Closed;
        public DateTimeOffset SignUpStart { get; set; }
        public string SignUpMessage { get; set; }
        public Guid? GameId { get; set; } = null;

        // Rewards
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
        public ulong? RoleReward { get; set; } = null;
    }
}