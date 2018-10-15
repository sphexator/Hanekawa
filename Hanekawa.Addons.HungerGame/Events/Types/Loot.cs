using System;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Data;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Loot
    {
        private const int FoodAndWater = 100;
        private const int Weapons = 15;
        private const int Bandages = 50;

        public static string LootEvent(HungerGameLive profile, DbService db)
        {
            var rand = new Random();
            const int pool = FoodAndWater + Weapons + Bandages;
            var result = rand.Next(1, pool);
            var user = db.HungerGameLives.Find(profile.UserId);
            if (result <= FoodAndWater)
            {
                string toReturn = null;
                var toGive = IsFood();
                switch (toGive)
                {
                    case 1:
                        user.Water = user.Water + 1;
                        db.SaveChanges();
                        toReturn = "Obtained Water";
                        break;
                    case 2:
                        user.Food = user.Food + 1;
                        db.SaveChanges();
                        toReturn = "Obtained Food";
                        break;
                    case 3:
                        user.Water = user.Water + 1;
                        user.Food = user.Food + 1;
                        db.SaveChanges();
                        toReturn = "Obtained Water and Food";
                        break;
                }

                return toReturn;
            }

            if (result <= FoodAndWater + Bandages) return $"Obtained {ConsumableNames.Bandages}";

            if (result > FoodAndWater + Bandages + Weapons) return WeaponNames.WeaponStrings[1];
            var weapon = rand.Next(0, 100);
            if (weapon <= 50)
            {
                user.Bow = user.Bow + 1;
                db.SaveChanges();
                return "Obtained bow";
                //Add Bow
            }

            if (weapon <= 50 + 30)
            {
                user.Axe = user.Axe + 1;
                db.SaveChanges();
                return "Obtained axe";
                //Add Axe
            }

            if (weapon <= 50 + 30 + 15)
            {
                user.Pistol = user.Pistol + 1;
                db.SaveChanges();
                return "Obtained pistol";
                //Add Pistol
            }

            if (weapon <= 50 + 30 + 15 + 15) return $"Obtained {WeaponNames.WeaponStrings[4]}";

            user.Bow = user.Bow + 1;
            db.SaveChanges();
            return "Obtained bow";
        }

        private static int IsFood()
        {
            var rand = new Random();
            var result = rand.Next(1, 3);
            return result;
        }
    }
}