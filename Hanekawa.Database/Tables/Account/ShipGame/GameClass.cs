namespace Hanekawa.Database.Tables.Account.ShipGame
{
    public class GameClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = "test";
        public uint LevelRequirement { get; set; } = 1;

        public int ChanceAvoid { get; set; } = 2;
        public int ChanceCrit { get; set; } = 2;

        public double ModifierHealth { get; set; } = 1.2;
        public double ModifierDamage { get; set; } = 1.2;
        public double ModifierAvoidance { get; set; } = 1.2;
        public double ModifierCriticalChance { get; set; } = 1.2;
    }
}