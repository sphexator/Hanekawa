using System;
using Hanekawa.Addons.Database.Tables;

namespace Hanekawa.Addons.HungerGame.Calculate
{
    internal class ChanceGenerator
    {
        private const int Loot = 400;
        private const int Kill = 100;
        private const int Idle = 200;
        private const int Meet = 100;
        private const int Hack = 25;
        private const int Die = 1;
        private const int Sleep = 1;
        private const int Eat = 1;

        public const string LootName = "Loot";
        public const string KillName = "Kill";
        public const string IdleName = "Idle";
        public const string MeetName = "Meet";
        public const string HackName = "Hack";
        public const string DieName = "Die";
        public const string SleepName = "Sleep";
        public const string EatName = "Eat";

        public static string EventDeterminator(HungerGameLive profile)
        {
            var loot = LootChance(profile);
            var kill = KillChance(profile);
            var idle = IdleChance(profile);
            var meet = MeetChance(profile);
            var hack = HackChance(profile);
            var die = DieChance(profile);
            var sleep = SleepChance(profile);
            var eat = EatChance(profile);

            var result = loot + kill + idle + meet + hack + die + sleep + eat;
            var rand = new Random();
            var generator = rand.Next(1, result);
            if (generator <= loot) return LootName;
            if (generator <= loot + kill) return KillName;
            if (generator <= loot + kill + idle) return IdleName;
            if (generator <= loot + kill + idle + meet) return MeetName;
            if (generator <= loot + kill + idle + meet + hack) return HackName;
            if (generator <= loot + kill + idle + meet + hack + die) return DieName;
            if (generator <= loot + kill + idle + meet + hack + die + sleep) return SleepName;
            return generator <= loot + kill + idle + meet + hack + die + sleep + eat ? EatName : null;
        }

        private static int LootChance(HungerGameLive profile)
        {
            if (profile.Water == 0) return Loot + 400;
            if (profile.Food == 0) return Loot + 400;
            if (profile.Water > 0 || profile.Food > 0 ||
                profile.TotalWeapons > 1) return Loot - 200;
            return Loot;
        }

        private static int KillChance(HungerGameLive profile)
        {
            if (profile.Water == 0 || profile.Food == 0)
                return Kill;
            if (profile.TotalWeapons >= 1 || profile.Water > 0 ||
                profile.Food > 0)
                return Kill + 2500;
            if (profile.TotalWeapons >= 1 && profile.Water > 0 ||
                profile.Food > 0)
                return Kill + 10000;
            return Kill;
        }

        private static int SleepChance(HungerGameLive profile)
        {
            if (profile.Sleep >= 90) return Sleep + 1000;
            if (profile.Sleep >= 75) return Sleep + 750;
            if (profile.Sleep >= 50) return Sleep + 500;
            return Sleep;
        }

        private static int EatChance(HungerGameLive profile)
        {
            if (profile.Hunger >= 90 || profile.Food > 0) return Eat + 1000;
            if (profile.Hunger >= 75 || profile.Food > 0) return Eat + 700;
            if (profile.Hunger >= 50 || profile.Food > 0) return Eat + 400;
            if (profile.Hunger >= 20 || profile.Food > 0) return Eat + 200;
            return Eat;
        }

        private static int IdleChance(HungerGameLive profile)
        {
            return Idle;
        }

        private static int MeetChance(HungerGameLive profile)
        {
            return Meet;
        }

        private static int HackChance(HungerGameLive profile)
        {
            return Hack;
        }

        private static int DieChance(HungerGameLive profile)
        {
            return Die;
        }
    }
}