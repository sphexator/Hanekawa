using System;
using System.Collections.Generic;
using System.Linq;
using Hanekawa.HungerGames.Entity.User;

namespace Hanekawa.HungerGames.Entity.Event
{
    internal class Attack
    {
        private readonly Random _random;

        internal Attack(Random random) => _random = random;

        internal UserAction AttackEvent(List<Participant> profiles, Participant profile, UserAction activity)
        {
            var target = GetTarget(profiles);
            // TODO: HOW TO GET WEAPON
            var weapon = GetBestWeapon(profile);
            if (weapon == null)
            {
                if (target.Health <= target.Health - 5)
                {
                    target.Health = 0;
                    target.Alive = false;
                }
                else
                {
                    target.Health -= 5;
                }
            }
            else
            {
                if (target.Health <= target.Health - weapon.GiveOrTake)
                {
                    target.Health = 0;
                    target.Alive = false;
                }
                else
                {
                    target.Health -= weapon.GiveOrTake;
                } 
                if(weapon.Ammo > 0) weapon.Ammo -= 1;
            }
            return activity;
        }

        private Participant GetTarget(IReadOnlyList<Participant> users)
        {
            var alive = users.Where(x => x.Alive && x.Health > 0);
            return users[_random.Next(alive.Count())];
        }

        private static Item GetBestWeapon(Participant profile)
        {
            Item weapon = null;
            var dmg = 0;
            foreach (var x in profile.Inventory.Where(x => x.Item.Type == ItemType.Weapon).ToList())
            {
                if (x.Item.GiveOrTake <= dmg) continue;
                if (x.Item.Ammo <= 0) continue;
                dmg = x.Item.GiveOrTake;
                weapon = x.Item;
            }

            return weapon;
        }
    }
}
