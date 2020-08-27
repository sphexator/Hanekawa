using System;
using Hanekawa.HungerGames.Entity.User;

namespace Hanekawa.HungerGames.Entity.Event
{
    internal class Loot
    {
        private const int FoodAndWater = 100;
        private const int Weapons = 15;
        private const int Bandages = 50;
        private readonly Random _random;

        internal Loot(Random random) => _random = random;

        internal UserAction LootEvent(Participant profile, ItemDrop items, UserAction activity)
        {
            const int pool = FoodAndWater + Weapons + Bandages;
            var result = _random.Next(pool);
            if (result <= FoodAndWater)
            {
                var toGive = IsFood();
                Drink drink;
                Food food;
                switch (toGive)
                {
                    case 1:
                        drink = items.Drinks[_random.Next(items.Drinks.Count)];
                        var drinkCheck = profile.Inventory.Drinks.FirstOrDefault(x => x.Drink == drink);

                        if (drinkCheck == null)
                            profile.Inventory.Drinks.Add(new DrinkInventory { Amount = 1, Drink = drink });
                        else drinkCheck.Amount += 1;
                        activity.Reward = drink;
                        return activity;
                    case 2:
                        food = items.Foods[_random.Next(items.Foods.Count)];
                        var foodCheck = profile.Inventory.Food.FirstOrDefault(x => x.Food == food);

                        if (foodCheck == null) profile.Inventory.Food.Add(new FoodInventory { Amount = 1, Food = food });
                        else foodCheck.Amount += 1;
                        activity.Reward = food;
                        return activity;
                    default:
                        return activity;
                }
            }

            if (result <= FoodAndWater + Bandages)
            {
                var firstAid = items.FirstAids[_random.Next(items.FirstAids.Count)];
                var firstAidCheck = profile.Inventory.FirstAid.FirstOrDefault(x => x.FirstAid == firstAid);
                if (firstAidCheck == null)
                    profile.Inventory.FirstAid.Add(new FirstAidInventory { Amount = 1, FirstAid = firstAid });
                else firstAidCheck.Amount += 1;
                activity.Reward = firstAid;
                return activity;
            }

            var weapon = items.Weapons[_random.Next(items.Weapons.Count)];
            var weaponCheck = profile.Inventory.Weapons.FirstOrDefault(x => x.Weapon == weapon);
            if (weaponCheck != null)
            {
                if (weaponCheck.Weapon.Ammo != null)
                {
                    weaponCheck.Weapon.Ammo += 10;
                }
                else
                {
                    weaponCheck.Amount += 1;
                }
            }
            else
            {
                profile.Inventory.Weapons.Add(new WeaponInventory { Amount = 1, Weapon = weapon });
            }

            activity.Reward = weapon;
            return activity;
        }

        private int IsFood() => _random.Next(1, 3);
    }
}
