using System;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Data;

namespace Jibril.Services.INC.Calculate
{
    public static class ChanceGenerator
    {
        private const int Loot = 400;
        private const int Kill = 100;
        private const int Idle = 200;
        private const int Meet = 100;
        private const int Hack = 50;
        private const int Die = 1;
        private const int Sleep = 1;
        private const int Eat = 1;

        public static string EventDeterminator(Profile profile)
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

        private static int LootChance(Profile profile)
        {
            if (profile.Consumables.TotalDrink == 0) return Loot + 400;
            if (profile.Consumables.TotalFood == 0) return Loot + 400;
            if (profile.Consumables.TotalDrink > 0 || profile.Consumables.TotalFood > 0 || profile.Weapons.TotalWeapons > 1) return Loot - 200;
            return Loot;
        }

        private static int KillChance(Profile profile)
        {
            if ((profile.Consumables.TotalDrink == 0 || profile.Consumables.TotalFood == 0))
                return 50;
            if (profile.Weapons.TotalWeapons >= 2 || (profile.Consumables.TotalDrink > 0 || profile.Consumables.TotalFood > 0))
                return Kill + 300;
            return Kill;
        }

        private static int SleepChance(Profile profile)
        {
            if (profile.Player.Sleep >= 90) return Sleep + 1000;
            if (profile.Player.Sleep >= 75) return Sleep + 750;
            if (profile.Player.Sleep >= 50) return Sleep + 500;

            return Sleep;
        }

        private static int EatChance(Profile profile)
        {
            if (profile.Player.Hunger >= 90 || profile.Consumables.TotalFood > 0) return Eat + 1000;
            if (profile.Player.Hunger >= 75 || profile.Consumables.TotalFood > 0) return Eat + 700;
            if (profile.Player.Hunger >= 50 || profile.Consumables.TotalFood > 0) return Eat + 400;
            if (profile.Player.Hunger >= 20 || profile.Consumables.TotalFood > 0) return Eat + 200;
            return Eat;
        }

        private static int IdleChance(Profile profile)
        {
            return Idle;
        }

        private static int MeetChance(Profile profile)
        {
            return Meet;
        }

        private static int HackChance(Profile profile)
        {
            return Hack;
        }

        private static int DieChance(Profile profile)
        {
            return Die;
        }

        public const string LootName = "Loot";
        public const string KillName = "Kill";
        public const string IdleName = "Idle";
        public const string MeetName = "Meet";
        public const string HackName = "Hack";
        public const string DieName = "Die";
        public const string SleepName = "Sleep";
        public const string EatName = "Eat";
    }
}
