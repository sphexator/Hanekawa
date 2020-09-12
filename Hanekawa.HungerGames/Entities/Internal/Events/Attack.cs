using System;
using System.Collections.Generic;
using System.Linq;
using Hanekawa.HungerGames.Entities.Items;

namespace Hanekawa.HungerGames.Entities.Internal.Events
{
    internal class Attack
    {
        private readonly Random _random;

        internal Attack(Random random) => _random = random;

        internal UserAction AttackEvent(List<HungerGameProfile> profiles, HungerGameProfile profile, UserAction activity)
        {
            var target = GetTarget(profiles);
            var weapon = GetBestWeapon(profile) ?? new Weapon();
            if (target.Health <= target.Health - weapon.Damage)
            {
                target.Health = 0;
                target.Alive = false;
            }
            else
            {
                target.Health -= weapon.Damage;
            }

            if (weapon.Ammo != null) weapon.Ammo -= 1;
            return activity;
        }

        private HungerGameProfile GetTarget(IReadOnlyList<HungerGameProfile> users)
        {
            var alive = users.Where(x => x.Alive && x.Health > 0);
            return users[_random.Next(alive.Count())];
        }

        private static Weapon GetBestWeapon(HungerGameProfile profile)
        {
            Weapon weapon = null;
            var dmg = 0;
            foreach (var x in profile.Inventory.Weapons)
            {
                if (x.Weapon.Damage <= dmg) continue;
                if (x.Weapon.Ammo == null && x.Weapon.Ammo <= 0) continue;
                dmg = x.Weapon.Damage;
                weapon = x.Weapon;
            }

            return weapon;
        }
    }
}