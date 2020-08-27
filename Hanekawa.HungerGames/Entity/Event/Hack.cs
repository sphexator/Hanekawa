using System;
using System.Collections.Generic;
using System.Text;
using Hanekawa.HungerGames.Entity.User;

namespace Hanekawa.HungerGames.Entity.Event
{
    internal class Hack
    {
        internal UserAction HackEvent(Participant profile, ItemDrop items, UserAction activity)
        {
            foreach (var x in items.Weapons)
            {
                var weaponCheck = profile.Inventory.Weapons.FirstOrDefault(y => y.Weapon == x);
                if (weaponCheck == null) profile.Inventory.Weapons.Add(new WeaponInventory { Amount = 1, Weapon = x });
                else weaponCheck.Amount += 1;
            }

            foreach (var x in items.FirstAids)
            {
                var firstAidCheck = profile.Inventory.FirstAid.FirstOrDefault(y => y.FirstAid == x);
                if (firstAidCheck == null)
                    profile.Inventory.FirstAid.Add(new FirstAidInventory { Amount = 1, FirstAid = x });
                else firstAidCheck.Amount += 1;
            }

            foreach (var x in items.Drinks)
            {
                var drinkCheck = profile.Inventory.Drinks.FirstOrDefault(y => y.Drink == x);
                if (drinkCheck == null) profile.Inventory.Drinks.Add(new DrinkInventory { Amount = 1, Drink = x });
                else drinkCheck.Amount += 1;
            }

            foreach (var x in items.Foods)
            {
                var foodCheck = profile.Inventory.Food.FirstOrDefault(y => y.Food == x);
                if (foodCheck == null) profile.Inventory.Food.Add(new FoodInventory { Amount = 1, Food = x });
                else foodCheck.Amount += 1;
            }

            activity.Reward = items;
            return activity;
        }
    }
}
