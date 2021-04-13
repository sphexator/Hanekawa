using System;
using System.Collections.Generic;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.HungerGames.Entities;
using Hanekawa.HungerGames.Entities.Configs;
using Hanekawa.HungerGames.Events;

namespace Hanekawa.HungerGames
{
    public partial class HungerGameClient
    {
        private readonly Random _random;
        private readonly HungerGameEvent _events;
        private readonly HungerGameConfig _config;

        public HungerGameClient(Random random, HungerGameConfig config)
        {
            _random = random;
            _config = config;
            _events = new HungerGameEvent(_random);
        }

        public List<UserAction> PlayAsync(List<HungerGameProfile> participants)
        {
            var results = new List<UserAction>();
            for (int i = 0; i < participants.Count; i++)
            {
                var x = participants[i];
                var before = x;
                var result = new UserAction
                {
                    Before = new HungerGameProfile
                    {
                        GuildId = x.GuildId,
                        UserId = x.UserId,
                        Bot = x.Bot,
                        Name = x.Name,
                        Avatar = x.Avatar,
                        Alive = x.Alive,
                        Health = x.Health,
                        Stamina = x.Stamina,
                        Bleeding = x.Bleeding,
                        Hunger = x.Hunger,
                        Thirst = x.Thirst,
                        Tiredness = x.Tiredness,
                        Move = x.Move,
                        Food = x.Food,
                        Water = x.Water,
                        FirstAid = x.FirstAid,
                        Weapons = x.Weapons,
                        MeleeWeapon = x.MeleeWeapon,
                        RangeWeapon = x.RangeWeapon,
                        Bullets = x.Bullets
                    }
                };
                if (!x.Alive)
                {
                    result.After = x;
                    results.Add(result);
                    continue;
                }

                var hgEvent = DeterminationEvent(x);
                switch (hgEvent)
                {
                    case ActionType.Loot:
                        result.Message = _events.Loot(x);
                        break;
                    case ActionType.Attack:
                        result.Message = _events.Attack(x, participants);
                        break;
                    case ActionType.Idle:
                        result.Message = _events.Idle();
                        break;
                    case ActionType.Meet:
                        result.Message = _events.Idle();
                        break;
                    case ActionType.Hack:
                        result.Message = _events.Hack(x);
                        break;
                    case ActionType.Die:
                        result.Message = _events.Die(x);
                        break;
                    case ActionType.Sleep:
                        result.Message = _events.Sleep(x);
                        break;
                    case ActionType.Eat:
                        result.Message = _events.EatAndDrink(x);
                        break;
                    case ActionType.None:
                        result.Message = _events.Idle();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (Fatigue(x, out var fatigue)) result.Message += $"\n{fatigue}";
                result.After = x;
                result.Action = hgEvent;
                results.Add(result);
            }

            return results;
        }

        private bool Fatigue(HungerGameProfile profile, out string fatigue)
        {
            profile.Hunger -= 10;
            profile.Tiredness -= 10;
            profile.Thirst -= 20;

            if ((profile.Hunger >= 100 || profile.Thirst >= 100) && profile.Bleeding)
            {
                if (profile.Hunger >= 100 && profile.Health >= 100)
                {
                    if (profile.Health - 50 <= 0)
                    {
                        profile.Health = 0;
                        profile.Alive = false;
                        fatigue = "Died from starvation, dehydration and bleeding out";
                        return true;
                    }
                    profile.Health -= 50;
                    fatigue = "Suffered from severe starvation, dehydration and bleeding out";
                    return true;
                }
                if (profile.Hunger >= 100)
                {
                    if (profile.Health - 30 <= 0)
                    {
                        profile.Health = 0;
                        profile.Alive = false;
                        fatigue = "Died from starvation and bleeding out";
                        return true;
                    }
                    profile.Health -= 30;
                    fatigue = "Suffered from severe starvation and bleeding out";
                    return true;
                }

                if (profile.Thirst >= 100)
                {
                    if (profile.Health - 40 <= 0)
                    {
                        profile.Health = 0;
                        profile.Alive = false;
                        fatigue = "Died from dehydration and bleeding out";
                        return true;
                    }
                    profile.Health -= 20;
                    fatigue = "Suffered from severe dehydration and bleeding out";
                    return true;
                }
            }

            if (profile.Bleeding)
            {
                if (profile.Health - 20 <= 0)
                {
                    profile.Health = 0;
                    profile.Alive = false;
                    fatigue = "Bled out and died";
                    return true;
                }
                profile.Health -= 20;
                fatigue = "Suffered from bleeding";
                return true;
            }

            fatigue = null;
            return false;
        }
    }
}
