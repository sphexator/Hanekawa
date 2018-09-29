namespace Hanekawa.Addons.Database.Tables
{
    public class HungerGameLive
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Fatigue { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Sleep { get; set; }
        public bool Bleeding { get; set; }
        public int Food { get; set; }
        public int Water { get; set; }
        public int TotalWeapons { get; set; }
        public int Pistol { get; set; }
        public int Axe { get; set; }
        public int Bow { get; set; }
    }
}