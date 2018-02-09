namespace Jibril.Services.INC.Data
{
    public class Profile
    {
        public Player Player { get; set; }
        public Weapons Weapons { get; set; }
        public Consumables Consumables { get; set; }
    }
    public abstract class Player
    {
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Damage { get; set; }
        public int Hunger { get; set; }
        public int Sleep { get; set; }
        public bool Status { get; set; }
    }
}