using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.HungerGames.Data;

namespace Jibril.Services.INC.Calculate
{
    public class ChanceGenerator
    {
        private const int Loot = 400;
        private const int Kill = 100;
        private const int Idle = 200;
        private const int Meet = 100;
        private const int Hack = 50;
        private const int Die = 1;
        private const int Sleep = 1;
        private const int Eat = 1;

        public static string EventDeterminator(Player player, Weapons weapons, Consumables consumables)
        {
            var loot = LootChance(player, weapons, consumables);
            var kill = KillChance(player, weapons, consumables);
            var idle = IdleChance(player, weapons, consumables);
            var meet = MeetChance(player, weapons, consumables);
            var hack = HackChance(player, weapons, consumables);
            var die = DieChance(player, weapons, consumables);
            var sleep = SleepChance(player, weapons, consumables);
            var eat = EatChance(player, weapons, consumables);

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

        private static int LootChance(Player player, Weapons weapons, Consumables consumables)
        {
            if (consumables.TotalDrink == 0) return Loot + 400;
            if (consumables.TotalFood == 0) return Loot + 400;
            if (consumables.TotalDrink > 0 || consumables.TotalFood > 0 || weapons.TotalWeapons > 1) return Loot - 200;
            return Loot;
        }

        private static int KillChance(Player player, Weapons weapons, Consumables consumables)
        {
            if ((consumables.TotalDrink == 0 || consumables.TotalFood == 0))
                return 50;
            if (weapons.TotalWeapons >= 2 || (consumables.TotalDrink > 0 || consumables.TotalFood > 0))
                return Kill + 300;
            return Kill;
        }

        private static int SleepChance(Player player, Weapons weapons, Consumables consumables)
        {
            if (player.Sleep >= 90) return Sleep + 1000;
            if (player.Sleep >= 75) return Sleep + 750;
            if (player.Sleep >= 50) return Sleep + 500;

            return Sleep;
        }

        private static int EatChance(Player player, Weapons weapons, Consumables consumables)
        {
            if (player.Hunger >= 90 || consumables.TotalFood > 0) return Eat + 1000;
            if (player.Hunger >= 75 || consumables.TotalFood > 0) return Eat + 700;
            if (player.Hunger >= 50 || consumables.TotalFood > 0) return Eat + 400;
            if (player.Hunger >= 20 || consumables.TotalFood > 0) return Eat + 200;
            return Eat;
        }

        private static int IdleChance(Player player, Weapons weapons, Consumables consumables)
        {
            return Idle;
        }

        private static int MeetChance(Player player, Weapons weapons, Consumables consumables)
        {
            return Meet;
        }

        private static int HackChance(Player player, Weapons weapons, Consumables consumables)
        {
            return Hack;
        }

        private static int DieChance(Player player, Weapons weapons, Consumables consumables)
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
