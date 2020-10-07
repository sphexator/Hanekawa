using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Bot.Services.Game.HungerGames
{
    public partial class HungerGame
    {
        private const int Loot = 400;
        private const int Kill = 100;
        private const int Idle = 200;
        private const int Meet = 100;
        private const int Hack = 25;
        private const int Die = 1;
        private const int Sleep = 1;
        private const int Eat = 1;

        private ActionType DeterminationEvent(HungerGameProfile profile)
        {
            var loot = LootChance(profile);
            var kill = KillChance(profile);
            var idle = IdleChance();
            var meet = MeetChance();
            var hack = HackChance();
            var die = DieChance();
            var sleep = SleepChance(profile);
            var eat = EatChance(profile);

            var result = loot + kill + idle + meet + hack + die + sleep + eat;
            var generator = _rand.Next(result);
            if (generator <= loot) return ActionType.Loot;
            if (generator <= loot + kill) return ActionType.Attack;
            if (generator <= loot + kill + idle) return ActionType.Idle;
            if (generator <= loot + kill + idle + meet) return ActionType.Meet;
            if (generator <= loot + kill + idle + meet + hack) return ActionType.Hack;
            if (generator <= loot + kill + idle + meet + hack + die) return ActionType.Die;
            else if (generator <= loot + kill + idle + meet + hack + die + sleep)
                return ActionType.Sleep;
            else
                return ActionType.Eat;
        }

        private static int LootChance(HungerGameProfile profile)
        {
            if (profile.Water == 0) return Loot + 400;
            if (profile.Food == 0) return Loot + 400;
            if (profile.MeleeWeapon == 0 || (profile.RangeWeapon == 0 && profile.Bullets == 0)) return Loot + 400;
            return Loot - 200;
        }

        private static int KillChance(HungerGameProfile profile)
        {
            if (profile.Water == 0 || profile.Food == 0)
                return 0;
            if (profile.Water == 1 || profile.Food == 1) return Kill;
            if (profile.Weapons >= 1 && (profile.Water > 2 ||
                                                         profile.Food > 2))
                return Kill + 10000;
            if (profile.Water > 1 || profile.Food > 1)
                return Kill + 1500;
            return Kill;
        }

        private static int SleepChance(HungerGameProfile profile)
        {
            if (profile.Tiredness >= 90) return Sleep + 1000;
            if (profile.Tiredness >= 75) return Sleep + 750;
            if (profile.Tiredness >= 50) return Sleep + 500;
            return Sleep;
        }

        private static int EatChance(HungerGameProfile profile)
        {
            if (profile.Hunger >= 20 && profile.Food >= 1) return Eat + 1000;
            if (profile.Hunger >= 50 && profile.Food >= 1) return Eat + 700;
            if (profile.Hunger >= 75 && profile.Food >= 1) return Eat + 400;
            if (profile.Hunger >= 90 && profile.Food >= 1) return Eat + 200;
            return Eat;
        }

        private static int IdleChance() => Idle;

        private static int MeetChance() => Meet;

        private static int HackChance() => Hack;

        private static int DieChance() => Die;
    }
}
