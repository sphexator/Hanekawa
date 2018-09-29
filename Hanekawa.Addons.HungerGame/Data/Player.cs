namespace Hanekawa.Addons.HungerGame.Data
{
    public class Profile
    {
        public Player Player { get; set; }
        public Weapons Weapons { get; set; }
        public Consumables Consumables { get; set; }
    }

    public class Player
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Damage { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Sleep { get; set; }
        public bool Status { get; set; }
        public bool Bleeding { get; set; }
    }
}