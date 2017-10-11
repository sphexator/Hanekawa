using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class EnemyStat
    {
        public static int HealthPoint(int level, int health)
        {
            var baseHealth = health + (13 * level);

            return baseHealth;
        }
        public static int DamagePoint(int level)
        {
            var baseDamage = 10 + (3 * level);
            return baseDamage;
        }
        public static int Avoidance(string shipClass, int level)
        {
            var classAvoidance = ClassStats.ClassAvoidance(shipClass);
            Random rand = new Random();
            int chance = rand.Next(1, 101);
            if (chance <= classAvoidance)
            {
                var damageTaken = 0;
                return damageTaken;
            }
            else
            {
                var damage = DamagePoint(level);
                return damage;
            }
        }
    }
}
