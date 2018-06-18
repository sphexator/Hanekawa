using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Loot
    {
        public static string LootEvent(Hungergame profile)
        {
            var rand = new Random();
            const int pool = FoodAndWater + Weapons + Bandages;
            var result = rand.Next(1, pool);
            using (var db = new hanekawaContext())
            {
                var user = db.Hungergame.Find(profile.Userid);
                if (result <= FoodAndWater)
                {
                    string toReturn = null;
                    var toGive = IsFood();
                    switch (toGive)
                    {
                        case 1:
                            user.Water = user.Water + 1;
                            user.Totaldrink = user.Totaldrink + 1;
                            db.SaveChanges();
                            toReturn = "Obtained Water";
                            break;
                        case 2:
                            user.Fish = user.Fish + 1;
                            user.Totalfood = user.Totalfood + 1;
                            db.SaveChanges();
                            toReturn = "Obtained Food";
                            break;
                        case 3:
                            user.Water = user.Water + 1;
                            user.Fish = user.Fish + 1;
                            user.Totaldrink = user.Totaldrink + 1;
                            user.Totalfood = user.Totalfood + 1;
                            db.SaveChanges();
                            toReturn = "Obtained Water and Food";
                            break;
                    }
                    return toReturn;

                }
                if (result <= FoodAndWater + Bandages)
                {
                    user.Bandages = user.Bandages + 1;
                    db.SaveChanges();
                    return $"Obtained {ConsumableNames.Bandages}";
                }

                if (result > FoodAndWater + Bandages + Weapons) return WeaponNames.WeaponStrings[1];
                var weapon = rand.Next(0, 100);
                if (weapon <= 50)
                {
                    user.Bow = user.Bow + 1;
                    user.Arrows = user.Arrows + 10;
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
                    user.Bullets = user.Bullets + 10;
                    db.SaveChanges();
                    return "Obtained pistol";
                    //Add Pistol
                }

                if (weapon <= 50 + 30 + 15 + 15)
                {
                    return $"Obtained {WeaponNames.WeaponStrings[4]}";
                    //Add Trap
                }

                user.Bow = user.Bow + 1;
                user.Arrows = user.Arrows + 10;
                db.SaveChanges();
                return $"Obtained bow";
            }
        }

        private static int IsFood()
        {
            var rand = new Random();
            var result = rand.Next(1, 3);
            return result;
        }

        private const int FoodAndWater = 100;
        private const int Weapons = 15;
        private const int Bandages = 50;
    }
}
