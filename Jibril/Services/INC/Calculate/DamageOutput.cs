using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.HungerGames.Data;

namespace Jibril.Services.INC.Calculate
{
    public class DamageOutput
    {
        public static int PistolDamage(int stamina, bool bleeding)
        {
            double hitChance = 75 * (stamina / 100);
            if (bleeding) hitChance = hitChance / 1.5;
            var rand = new Random();
            var chance = rand.Next(0, 100);
            if (!(chance <= hitChance)) return 0;
            var damage = rand.Next((Pistol.Damage - 20), Pistol.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            return damage;
        }
        private static bool CriticalDamage()
        {
            var rand = new Random();
            var chance = rand.Next(0, 100);
            return chance >= 70;
        }
    }
}
