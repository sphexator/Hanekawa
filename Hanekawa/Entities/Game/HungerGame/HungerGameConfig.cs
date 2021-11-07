using System;

namespace Hanekawa.Entities.Game.HungerGame
{
    public class HungerGameConfig
    {
        public HungerGameConfig() { }
        public HungerGameConfig(ulong guildId) => GuildId = guildId;

        public ulong GuildId { get; set; }
        public ulong? SignUpChannel { get; set; } = null;
        public ulong? EventChannel { get; set; } = null;
        public string EmoteMessageFormat { get; set; } 

        public string SignUpMessage { get; set; }
        public Guid? GameId { get; set; } = null;

        // Rewards
        public int ExpReward { get; set; } = 0;
        public int CreditReward { get; set; } = 0;
        public int SpecialCreditReward { get; set; } = 0;
        public ulong? RoleReward { get; set; } = null;
    }
}