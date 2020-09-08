namespace Hanekawa.HungerGames.Entities.User
{
    public class HungerGameProfile
    {
        public string Name { get; set; } = "Test Unit";
        public string Avatar { get; set; }
        public bool Alive { get; set; } = true;
        public double Health { get; set; } = 100;
        public double Stamina { get; set; } = 100;
        public double Hunger { get; set; } = 100;
        public double Thirst { get; set; } = 100;
        public double Tiredness { get; set; }
        public bool Bleeding { get; set; } = false;
        public PlayerInventory Inventory { get; set; } = new PlayerInventory();
        public ActionType? Move { get; set; } = null;
    }
}