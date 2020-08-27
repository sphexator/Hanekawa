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
            if (profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) == 0 ||
                profile.Inventory.Count(x => x.Item.Type == ItemType.Food) == 0)
                return 0;
            if (profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) == 1 ||
                profile.Inventory.Count(x => x.Item.Type == ItemType.Food) == 1) 
                return Kill;
            if (profile.Inventory.Count(x => x.Item.Type == ItemType.Weapon) >= 1 &&
                (profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) > 2 ||
                 profile.Inventory.Count(x => x.Item.Type == ItemType.Food) > 2))
                return Kill + 10000;
            if (profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) > 1 ||
                profile.Inventory.Count(x => x.Item.Type == ItemType.Food) > 1)
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

        private static int IdleChance() => Idle;

        private static int HackChance() => Hack;

        private static int DieChance() => Die;
    }
}
