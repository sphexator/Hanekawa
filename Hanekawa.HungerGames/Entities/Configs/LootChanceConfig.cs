namespace Hanekawa.HungerGames.Entities.Configs
{
    public class LootChanceConfig
    {
        public int LootChance { get; set; } = 400;
        public int KillChance { get; set; } = 100;
        public int IdleChance { get; set; } = 200;
        public int MeetChance { get; set; } = 25;
        public int HackChance { get; set; } = 1;
        public int DieChance { get; set; } = 1;
        public int SleepChance { get; set; } = 1;
        public int EatChance { get; set; } = 1;
    }
}