using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.INC.Data;

namespace Jibril.Services.INC.Events.Types
{
    public class Loot
    {
        public static string LootEvent()
        {
            var rand = new Random();
            const int pool = FoodAndWater + Weapons + Bandages;
            var result = rand.Next(1, pool);

            if (result <= FoodAndWater)
            {
                string toReturn = null;
                var toGive = IsFood();
                switch (toGive)
                {
                    case 1:
                        toReturn = "Give Water";
                        break;
                    case 2:
                        toReturn = "Obtained Food";
                        break;
                    case 3:
                        toReturn = "Obtained Water and Food";
                        break;
                }
                return toReturn;

                //TODO: implement add water and food to DB
            }
            if (result <= FoodAndWater + Bandages)
            {
                return ConsumableNames.Bandages;
                //TODO: implement add bandages DB
            }

            if (result > FoodAndWater + Bandages + Weapons) return WeaponNames.WeaponStrings[1];
            var looted = new List<LootReturn>();
            var weapon = rand.Next(0, 100);
            if (weapon <= 50)
            {
                return WeaponNames.WeaponStrings[1];
                //Add Bow
            }
            if (weapon <= 50 + 30)
            {
                return WeaponNames.WeaponStrings[2];
                //Add Axe
            }
            if (weapon <= 50 + 30 + 15)
            {
                return WeaponNames.WeaponStrings[3];
                //Add pistol
            }
            return weapon <= 50 + 30 + 15 + 15 ? WeaponNames.WeaponStrings[4] : WeaponNames.WeaponStrings[1];

            //TODO: implement add weapon to DB
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
    public abstract class LootReturn
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}
