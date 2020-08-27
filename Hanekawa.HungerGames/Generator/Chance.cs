using System;
using System.Linq;
using Hanekawa.HungerGames.Entity;

namespace Hanekawa.HungerGames.Generator
{
    internal class Chance 
    {
        private const int Loot = 400;
        private const int Kill = 100;
        private const int Idle = 200;
        private const int Hack = 25;
        private const int Die = 1;
        private const int Sleep = 1;
        private readonly Random _rand;

        internal Chance(Random rand) => _rand = rand;

        internal ActionType EventDetermination(Participant profile)
        {
            var loot = LootChance(profile);
            var kill = KillChance(profile);
            var idle = IdleChance();
            var hack = HackChance();
            var die = DieChance();
            var sleep = SleepChance(profile);
            var eat = EatChance(profile);

            var result = loot + kill + idle + hack + die + sleep + eat;
            var generator = _rand.Next(result);
            if (generator <= loot) return ActionType.Loot;
            if (generator <= loot + kill) return ActionType.Attack;
            if (generator <= loot + kill + idle) return ActionType.Idle;
            if (generator <= loot + kill + idle + hack) return ActionType.Hack;
            if (generator <= loot + kill + idle + hack + die) return ActionType.Die;
            return generator <= loot + kill + idle + hack + die + sleep ? ActionType.Sleep : ActionType.Idle;
        }

        private static int LootChance(Participant profile)
        {
            var drinks = profile.Inventory.FirstOrDefault();
            var food = profile.Inventory.FirstOrDefault();
            if (drinks == null || food == null) return Loot + 400;
            return Loot - 200;
        }

        private static int KillChance(Participant profile)
        {
            if (profile.Inventory.Drinks.Count == 0 || profile.Inventory.Food.Count == 0)
                return 0;
            if (profile.Inventory.Drinks.Count == 1 || profile.Inventory.Food.Count == 1) return Kill;
            if (profile.Inventory.Weapons.Count >= 1 && (profile.Inventory.Drinks.Count > 2 ||
                                                         profile.Inventory.Food.Count > 2))
                return Kill + 10000;
            if (profile.Inventory.Drinks.Count > 1 || profile.Inventory.Food.Count > 1)
                return Kill + 1500;
            return Kill;
        }

        private static int SleepChance(Participant profile)
        {
            if (profile.Tiredness >= 90) return Sleep + 1000;
            if (profile.Tiredness >= 75) return Sleep + 750;
            if (profile.Tiredness >= 50) return Sleep + 500;
            return Sleep;
        }

        private static int EatChance(Participant profile)
        {
            if (profile.Hunger >= 20 && profile.Inventory.Food.Any()) return Eat + 1000;
            if (profile.Hunger >= 50 && profile.Inventory.Food.Any()) return Eat + 700;
            if (profile.Hunger >= 75 && profile.Inventory.Food.Any()) return Eat + 400;
            if (profile.Hunger >= 90 && profile.Inventory.Food.Any()) return Eat + 200;
            return Eat;
        }

        private static int IdleChance() => Idle;

        private static int HackChance() => Hack;

        private static int DieChance() => Die;
    }
}
