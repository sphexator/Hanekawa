namespace Hanekawa.Addons.Database.Tables.BotGame
{
    public class GameClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public uint LevelRequirement { get; set; }

        public int ChanceAvoid { get; set; }
        public int ChanceCrit { get; set; }

        public double ModifierHealth { get; set; }
        public double ModifierDamage { get; set; }
        public double ModifierAvoidance { get; set; }
        public double ModifierCriticalChance { get; set; }
    }
}