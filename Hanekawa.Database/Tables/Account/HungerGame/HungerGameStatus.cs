using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameStatus
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? SignUpChannel { get; set; } = null;
        public Snowflake? EventChannel { get; set; } = null;
        public string EmoteMessageFormat { get; set; } 

        public GameStage Stage { get; set; } = GameStage.Closed;
        public DateTimeOffset SignUpStart { get; set; }
        public string SignUpMessage { get; set; }
        public Guid? GameId { get; set; } = null;

        // Rewards
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
        public Snowflake? RoleReward { get; set; } = null;
    }
}