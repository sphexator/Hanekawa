namespace Hanekawa.HungerGames.Entities
{
    public class HungerGameConfig
    {
        public Fatigue Fatigue { get; set; } = new Fatigue();
        public int ChanceToDamage { get; set; } = 1;
        public int ChanceToLoot { get; set; } = 1;
        public int ChanceToSleep { get; set; } = 1;
        public int ChanceToIdle { get; set; } = 1;
    }
}