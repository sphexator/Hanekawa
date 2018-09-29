namespace Hanekawa.Addons.HungerGame.Data
{
    public class Weapons
    {
        public int TotalWeapons { get; set; }
        public int Pistol { get; set; }
        public int Bullets { get; set; }
        public int Bow { get; set; }
        public int Arrows { get; set; }
        public int Axe { get; set; }
        public int Trap { get; set; }
    }

    public static class Pistol
    {
        public const int Damage = 80;
    }

    public static class Bow
    {
        public const int Damage = 60;
    }

    public static class Axe
    {
        public const int Damage = 70;
    }

    public static class Trap
    {
        public const int Damage = 90;
    }

    public static class Fist
    {
        public const int Damage = 40;
    }

    public static class WeaponNames
    {
        public static readonly string[] WeaponStrings =
        {
            "Bow",
            "Axe",
            "Pistol",
            "Trap"
        };
    }
}