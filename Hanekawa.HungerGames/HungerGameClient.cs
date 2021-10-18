using System;
using System.Collections.Generic;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.HungerGames.Entities;
using Hanekawa.HungerGames.Entities.Configs;
using Hanekawa.HungerGames.Events;
using HungerGameConfig = Hanekawa.HungerGames.Entities.Configs.HungerGameConfig;

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
            foreach (var x in participants)
            {
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
                result.Message = hgEvent switch
                {
                    ActionType.Loot => _events.Loot(x),
                    ActionType.Attack => _events.Attack(x, participants),
                    ActionType.Idle => _events.Idle(),
                    ActionType.Meet => _events.Idle(),
                    ActionType.Hack => _events.Hack(x),
                    ActionType.Die => _events.Die(x),
                    ActionType.Sleep => _events.Sleep(x),
                    ActionType.Eat => _events.EatAndDrink(x),
                    ActionType.None => _events.Idle(),
                    _ => throw new ArgumentOutOfRangeException("No matching event in event determine switch", (Exception) null)
                };

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
