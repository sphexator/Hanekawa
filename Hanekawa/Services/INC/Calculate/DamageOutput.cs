using System;
using Hanekawa.Services.INC.Data;

namespace Hanekawa.Services.INC.Calculate
{
    public class DamageOutput
    {
        public static int PistolDamage(uint stamina, bool bleeding)
        {
            var rand = new Random();
            var damage = rand.Next(Pistol.Damage - 20, Pistol.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            if (bleeding) return damage / 2;
            return damage;
        }

        public static int BowDamage(uint stamina, bool bleeding)
        {
            var rand = new Random();
            var damage = rand.Next(Bow.Damage - 20, Bow.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            if (bleeding) return damage / 2;
            return damage;
        }

        public static int AxeDamage(uint stamina, bool bleeding)
        {
            var rand = new Random();
            var damage = rand.Next(Axe.Damage - 20, Axe.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            if (bleeding) return damage / 2;
            return damage;
        }

        public static int TrapDamage(uint stamina, bool bleeding)
        {
            var rand = new Random();
            var damage = rand.Next(Trap.Damage - 20, Trap.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            if (bleeding) return damage / 2;
            return damage;
        }

        public static int FistDamage(uint stamina, bool bleeding)
        {
            var rand = new Random();
            var damage = rand.Next(Fist.Damage - 10, Fist.Damage);
            var critChance = CriticalDamage();
            if (critChance) damage = damage + rand.Next(10, 20);
            if (bleeding) return damage / 2;
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