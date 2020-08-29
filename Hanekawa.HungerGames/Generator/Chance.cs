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
            //var eat = EatChance(profile);

            var result = loot + kill + idle + hack + die + sleep;
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
            return drinks == null || food == null ? Loot + 400 : Loot - 200;
        }

        private static int KillChance(Participant profile) =>
            profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) == 0 ||
            profile.Inventory.Count(x => x.Item.Type == ItemType.Food) == 0
                ? 0
                : profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) == 1 ||
                  profile.Inventory.Count(x => x.Item.Type == ItemType.Food) == 1
                    ? Kill
                    : profile.Inventory.Count(x => x.Item.Type == ItemType.Weapon) >= 1 &&
                      (profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) > 2 ||
                       profile.Inventory.Count(x => x.Item.Type == ItemType.Food) > 2)
                        ? Kill + 10000
                        : profile.Inventory.Count(x => x.Item.Type == ItemType.Drink) > 1 ||
                          profile.Inventory.Count(x => x.Item.Type == ItemType.Food) > 1
                            ? Kill + 1500
                            : Kill;

        private static int SleepChance(Participant profile) =>
            profile.Tiredness >= 90 ? Sleep + 1000 :
            profile.Tiredness >= 75 ? Sleep + 750 :
            profile.Tiredness >= 50 ? Sleep + 500 : Sleep;

        private static int IdleChance() => Idle;

        private static int HackChance() => Hack;

        private static int DieChance() => Die;
    }
}
