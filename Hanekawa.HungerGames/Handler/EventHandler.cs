using System.Collections.Generic;
using Hanekawa.HungerGames.Entity;
using Hanekawa.HungerGames.Entity.Event;
using Hanekawa.HungerGames.Entity.User;
using Hanekawa.HungerGames.Generator;

namespace Hanekawa.HungerGames.Handler
{
    internal class EventHandler
    {
        private readonly Attack _attack;
        private readonly Chance _chance;
        private readonly Die _die;
        private readonly Hack _hack;
        private readonly Loot _loot;
        private readonly Sleep _sleep;

        internal EventHandler(Chance chance, Loot loot, Attack attack, Hack hack, Die die,
            Sleep sleep)
        {
            _chance = chance;
            _loot = loot;
            _attack = attack;
            _hack = hack;
            _die = die;
            _sleep = sleep;
        }

        internal void DetermineEvent(List<Participant> users, Participant profile, UserAction activity) =>
            EventManager(_chance.EventDetermination(profile), users, profile, activity);

        internal void DetermineEvent(ActionType type, List<Participant> users, Participant profile, UserAction activity) =>
            EventManager(type, users, profile, activity);

        private UserAction EventManager(ActionType type, List<Participant> users, Participant profile, UserAction activity)
        {
            switch (type)
            {
                case ActionType.Loot:
                {
                    return _loot.LootEvent(profile, activity);
                }
                case ActionType.Attack:
                {
                    activity.Action = ActionType.Attack;
                    return _attack.AttackEvent(users, profile, activity);
                }
                case ActionType.Idle:
                {
                    activity.Action = ActionType.Idle;
                    return activity;
                }
                case ActionType.Hack:
                {
                    activity.Action = ActionType.Hack;
                    return _hack.HackEvent(profile, activity);
                }
                case ActionType.Die:
                {
                    activity.Action = ActionType.Die;
                    _die.DieEvent(profile);
                    return activity;
                }
                case ActionType.Sleep:
                {
                    activity.Action = ActionType.Sleep;
                    _sleep.SleepEvent(profile);
                    return activity;
                }
                default:
                    activity.Action = ActionType.Idle;
                    return activity;
            }
        }
    }
}
