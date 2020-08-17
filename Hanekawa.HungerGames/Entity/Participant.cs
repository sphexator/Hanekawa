using System.Collections.Generic;

namespace Hanekawa.HungerGames.Entity
{
    public class Participant
    {
        public Participant(ulong guildId, ulong userId)
        {
            GuildId = guildId;
            UserId = userId;
        }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string Name { get; set; } = "Test Unit";
        public string Avatar { get; set; } = null;
        public bool Alive { get; set; } = true;
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public int Hunger { get; set; } = 0;
        public int Thirst { get; set; } = 0;
        public int Tiredness { get; set; } = 0;
        public bool Bleeding { get; set; } = false;
        public ActionType LatestMove { get; set; } = ActionType.Idle;

        public List<Inventory> Inventory { get; set; }
    }
}