namespace Hanekawa.HungerGames
{
    public class HungerGameConfig
    {
        public int ChanceToDamage { get; set; } = 1;
        public int ChanceToLoot { get; set; } = 1;
        public int ChanceToSleep { get; set; } = 1;
        public int ChanceToIdle { get; set; } = 1;

        public int HungerRate { get; set; } = 1;
        public int ThirstRate { get; set; } = 1;
        public int ExhaustionRate { get; set; } = 1;
        public int StaminaRate { get; set; } = 1;
    }
}