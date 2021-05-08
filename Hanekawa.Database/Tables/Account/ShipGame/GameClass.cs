using System;

namespace Hanekawa.Database.Tables.Account.ShipGame
{
    public class GameClass
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "test";
        public int LevelRequirement { get; set; } = 1;

        public int ChanceAvoid { get; set; } = 2;
        public int ChanceCrit { get; set; } = 2;

        public double ModifierHealth { get; set; } = 1.2;
        public double ModifierDamage { get; set; } = 1.2;
        public double ModifierAvoidance { get; set; } = 1.2;
        public double ModifierCriticalChance { get; set; } = 1.2;
    }
}