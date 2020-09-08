using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using HungerGame.Entities;
using HungerGame.Entities.Internal;
using HungerGame.Entities.Items;
using HungerGame.Entities.User;
using HungerGame.Generator;

namespace HungerGame.Handler
{
    internal class GameHandler : IRequired
    {
        private readonly EventHandler _eventHandler;
        private readonly ImageGenerator _generator;
        private readonly Random _random;

        internal GameHandler(Random random, ImageGenerator generator, EventHandler eventHandler)
        {
            _random = random;
            _generator = generator;
            _eventHandler = eventHandler;
        }

        internal async Task<IEnumerable<HgResult>> CustomRoundAsync(List<HungerGameProfile> profiles, ItemDrop itemDrops)
        {
            var result = new List<HgResult>();
            foreach (var x in profiles)
            {
                var activity = new UserAction { BeforeProfile = x };
                if (x.Alive)
                {
                    if (x.Move.HasValue) _eventHandler.DetermineEvent(x.Move.Value, profiles, x, itemDrops, activity);
                    else _eventHandler.DetermineEvent(profiles, x, itemDrops, activity);
                    Fatigue(x);
                    SelfHarm(x);
                }

                activity.AfterProfile = x;
                result.Add(new HgResult
                {
                    Action = activity,
                    Image = await _generator.GenerateSingleImageAsync(x)
                });
            }
            return result;
        }

        internal async Task<HgOverallResult> DefaultRoundAsync(List<HungerGameProfile> profiles, ItemDrop itemDrops)
        {
            var result = new HgOverallResult
            {
                Participants = profiles,
                Image = await _generator.GenerateEventImageAsync(profiles)
            };
            var userActions = new List<UserAction>();
            foreach (var x in profiles)
            {
                var activity = new UserAction { BeforeProfile = x };
                if (x.Alive)
                {
                    if (x.Move.HasValue) _eventHandler.DetermineEvent(x.Move.Value, profiles, x, itemDrops, activity);
                    else _eventHandler.DetermineEvent(profiles, x, itemDrops, activity);
                    Fatigue(x);
                    SelfHarm(x);
                }
                activity.AfterProfile = x;
                userActions.Add(activity);
            }

            result.UserAction = userActions;
            return result;
        }

        private void SelfHarm(HungerGameProfile profile)
        {
            if (!profile.Alive) return;
            int dmg;
            if (profile.Hunger >= 90 || profile.Tiredness >= 100) dmg = _random.Next(20, 30);
            else if (profile.Hunger >= 80 || profile.Tiredness >= 90) dmg = _random.Next(5, 10);
            else return;
            if (profile.Health - dmg <= 0)
            {
                profile.Alive = false;
                profile.Health = 0;
            }
            else
            {
                profile.Health = profile.Health - dmg;
            }
        }

        private void Fatigue(HungerGameProfile profile)
        {
            if (!profile.Alive) return;
            profile.Hunger = profile.Hunger + _random.Next(5, 10);
            profile.Tiredness = profile.Tiredness + _random.Next(20, 30);
        }
    }
}