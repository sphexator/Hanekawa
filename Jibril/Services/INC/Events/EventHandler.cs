using System;
using System.Collections.Generic;
using System.Linq;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;

namespace Jibril.Services.INC.Events
{
    public class EventHandler
    {
        public static void EventManager(Player player, Weapons weapons, Consumables consumables)
        {
            var evt = ChanceGenerator.EventDeterminator(player, weapons, consumables);
            if (evt == ChanceGenerator.LootName)
            {

            }
            if (evt == ChanceGenerator.KillName)
            {

            }
            if (evt == ChanceGenerator.IdleName)
            {

            }
            if (evt == ChanceGenerator.MeetName)
            {

            }
            if (evt == ChanceGenerator.HackName)
            {

            }
            if (evt == ChanceGenerator.DieName)
            {

            }
            if (evt == ChanceGenerator.SleepName)
            {

            }
            if (evt == ChanceGenerator.EatName)
            {

            }
        }

        private static string LootEvent()
        {
            var rand = new Random();
            const int pool = FoodAndWater + Weapons + Bandages;
            var result = rand.Next(1, pool);

            if (result <= FoodAndWater)
            {
                //TODO: implement add water and food to DB
            }
            if (result <= FoodAndWater + Bandages)
            {
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