using System;
using System.Collections.Generic;
using System.Linq;
using Hanekawa.Bot.Services.Game.HungerGames.Events;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Models.HungerGame;
using Hanekawa.Shared.Game.HungerGame;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Bot.Services.Game.HungerGames
{
    public partial class HungerGame : INService
    {
        private readonly Random _rand;
        private readonly HgEvent _events;
        public HungerGame(Random rand, HgEvent events)
        {
            _rand = rand;
            _events = events;
        }

        public List<UserAction> PlayAsync(List<HungerGameProfile> participants)
        {
            var results = new List<UserAction>();
            for (var i = 0; i < participants.Count; i++)
            {
                var x = participants[i];
                var result = new UserAction { BeforeProfile = new HungerGameProfile
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
                } };
                if (!x.Alive)
                {
                    result.AfterProfile = x;
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

                var fatigue = Fatigue(x);
                if (fatigue != null)
                {
                    result.Message += $"\n{fatigue}";
                }
                result.AfterProfile = x;
                result.Action = hgEvent;
                results.Add(result);
            }

            return results;
        }

        private string Fatigue(HungerGameProfile profile)
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
                        return "Died from starvation, dehydration and bleeding out";
                    }
                    profile.Health -= 50;
                    return "Suffered from severe starvation, dehydration and bleeding out";
                }
                if (profile.Hunger >= 100)
                {
                    if (profile.Health - 30 <= 0)
                    {
                        profile.Health = 0;
                        profile.Alive = false;
                        return "Died from starvation and bleeding out";
                    }
                    profile.Health -= 30;
                    return "Suffered from severe starvation and bleeding out";
                }

                if (profile.Thirst >= 100)
                {
                    if (profile.Health - 40 <= 0)
                    {
                        profile.Health = 0;
                        profile.Alive = false;
                        return "Died from dehydration and bleeding out";
                    }
                    profile.Health -= 20;
                    return "Suffered from severe dehydration and bleeding out";
                }
            }

            if (profile.Bleeding)
            {
                if (profile.Health - 20 <= 0)
                {
                    profile.Health = 0;
                    profile.Alive = false;
                    return "Bled out and died";
                } 
                profile.Health -= 20; 
                return "Suffered from bleeding";
            }

            return null;
        }

        private void HungerAndOrThirst(){}
    }
}
