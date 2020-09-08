namespace HungerGame.Entities.Items
{
    public class Weapon
    {
        public string Name { get; set; } = "Fist";
        public int Damage { get; set; } = 30;
        public int? Ammo { get; set; } = null;
        public bool BleedEffect { get; set; } = false;
    }
}