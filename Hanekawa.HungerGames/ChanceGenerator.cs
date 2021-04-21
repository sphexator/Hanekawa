using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Account.HungerGame;

namespace Hanekawa.HungerGames
{
    public partial class HungerGameClient
    {
        private ActionType DeterminationEvent(HungerGameProfile profile)
        {
            var loot = LootChance(profile);
            var kill = KillChance(profile);
            var idle = IdleChance(profile);
            var meet = MeetChance(profile);
            var hack = HackChance(profile);
            var die = DieChance();
            var sleep = SleepChance(profile);
            var eat = EatChance(profile);

            var result = loot + kill + idle + meet + hack + die + sleep + eat;
            var generator = _random.Next(result);
            if (generator <= loot) return ActionType.Loot;
            if (generator <= loot + kill) return ActionType.Attack;
            if (generator <= loot + kill + idle) return ActionType.Idle;
            if (generator <= loot + kill + idle + meet) return ActionType.Meet;
            if (generator <= loot + kill + idle + meet + hack) return ActionType.Hack;
            if (generator <= loot + kill + idle + meet + hack + die) return ActionType.Die;
            return generator <= loot + kill + idle + meet + hack + die + sleep ? ActionType.Sleep : ActionType.Eat;
        }

        private int LootChance(HungerGameProfile profile) =>
            profile.Water == 0
                ? _config.LootChance.LootChance + 400
                : profile.Food == 0
                    ? _config.LootChance.LootChance + 400
                    : profile.MeleeWeapon == 0 || (profile.RangeWeapon == 0 && profile.Bullets == 0)
                        ? _config.LootChance.LootChance + 400
                        : 0;

        private int KillChance(HungerGameProfile profile) =>
            profile.Water == 0 || profile.Food == 0
                ? 0
                : profile.Water == 1 || profile.Food == 1
                    ? _config.LootChance.KillChance
                    : profile.Weapons >= 1 && (profile.Water > 2 ||
                                               profile.Food > 2)
                        ? _config.LootChance.KillChance + 10000
                        : profile.Water > 1 || profile.Food > 1
                            ? _config.LootChance.KillChance + 1500
                            : _config.LootChance.KillChance;

        private int SleepChance(HungerGameProfile profile) =>
            profile.Tiredness >= 90 ? _config.LootChance.SleepChance + 1000 :
            profile.Tiredness >= 75 ? _config.LootChance.SleepChance + 750 :
            profile.Tiredness >= 50 ? _config.LootChance.SleepChance : 0;

        private int EatChance(HungerGameProfile profile) =>
            profile.Hunger >= 20 && profile.Food >= 1 ? _config.LootChance.EatChance + 1000 :
            profile.Hunger >= 50 && profile.Food >= 1 ? _config.LootChance.EatChance + 500 :
            profile.Hunger >= 75 && profile.Food >= 1 ? _config.LootChance.EatChance :
            profile.Hunger >= 90 && profile.Food >= 1 ? 0 : _config.LootChance.EatChance;

        private int IdleChance(HungerGameProfile profile) => profile.Move == ActionType.Idle ? 0 : _config.LootChance.IdleChance;

        private int MeetChance(HungerGameProfile profile) => profile.Move == ActionType.Meet ? 0 : _config.LootChance.MeetChance;

        private int HackChance(HungerGameProfile profile) => profile.Move == ActionType.Hack ? 0 : _config.LootChance.HackChance;

        private int DieChance() => _config.LootChance.DieChance;
    }
}
