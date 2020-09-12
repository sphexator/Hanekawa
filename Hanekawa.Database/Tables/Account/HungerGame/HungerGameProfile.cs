using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameProfile
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        
        public string Name { get; set; } = "Test Unit";
        public string Avatar { get; set; } = null;
        
        public bool Alive { get; set; } = true;
        public double Health { get; set; } = 100;
        public double Stamina { get; set; } = 100;
        public bool Bleeding { get; set; } = false;

        public double Hunger { get; set; } = 100;
        public double Thirst { get; set; } = 100;
        public double Tiredness { get; set; } = 0;
        public ActionType Move { get; set; } = ActionType.None;

        public List<> Inventory { get; set; }
    }
}