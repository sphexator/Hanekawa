using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameProfile
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool Bot { get; set; } = false;
        
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

        public int Food { get; set; } = 0;
        public int Water { get; set; } = 0;
        public int FirstAid { get; set; } = 0;
        public int Weapons { get; set; } = 0;
        public int MeleeWeapon { get; set; } = 0;
        public int RangeWeapon { get; set; } = 0;
        public int Bullets { get; set; } = 0;
    }
}