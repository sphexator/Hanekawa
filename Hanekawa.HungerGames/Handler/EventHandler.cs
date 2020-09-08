﻿using System.Collections.Generic;
using HungerGame.Entities.Internal;
using HungerGame.Entities.Internal.Events;
using HungerGame.Entities.Items;
using HungerGame.Entities.User;
using HungerGame.Generator;

namespace HungerGame.Handler
{
    internal class EventHandler : IRequired
    {
        private readonly Attack _attack;
        private readonly ChanceGenerator _chance;
        private readonly Die _die;
        private readonly Consume _eat;
        private readonly Hack _hack;
        private readonly Loot _loot;
        private readonly Sleep _sleep;

        internal EventHandler(ChanceGenerator chance, Loot loot, Attack attack, Hack hack, Die die,
            Sleep sleep, Consume eat)
        {
            _chance = chance;
            _loot = loot;
            _attack = attack;
            _hack = hack;
            _die = die;
            _sleep = sleep;
            _eat = eat;
        }

        internal void DetermineEvent(List<HungerGameProfile> users, HungerGameProfile profile, ItemDrop drops, UserAction activity) =>
            EventManager(_chance.EventDetermination(profile), users, profile, drops, activity);

        internal void DetermineEvent(ActionType type, List<HungerGameProfile> users, HungerGameProfile profile,
            ItemDrop drops, UserAction activity) =>
            EventManager(type, users, profile, drops, activity);

        private UserAction EventManager(ActionType type, List<HungerGameProfile> users, HungerGameProfile profile,
            ItemDrop drops, UserAction activity)
        {
            switch (type)
            {
                case ActionType.Loot:
                {
                    return _loot.LootEvent(profile, drops, activity);
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
                case ActionType.Meet:
                {
                    activity.Action = ActionType.Meet;
                    return activity;
                }
                case ActionType.Hack:
                {
                    activity.Action = ActionType.Hack;
                    return _hack.HackEvent(profile, drops, activity);
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
                case ActionType.Eat:
                {
                    activity.Action = ActionType.Eat;
                    _eat.EatEvent(profile);
                    return activity;
                }
                default:
                    activity.Action = ActionType.Idle;
                    return activity;
            }
        }
    }
}