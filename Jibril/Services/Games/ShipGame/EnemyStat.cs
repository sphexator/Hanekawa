using System;

namespace Jibril.Services.Games.ShipGame
{
    public class EnemyStat
    {
        public static int HealthPoint(int level, int health)
        {
            var baseHealth = health + 13 * level;

            return baseHealth;
        }

        public static int DamagePoint(int level)
        {
            var baseDamage = 10 + 3 * level;
            return baseDamage;
        }

        public static int Avoidance(string shipClass, int level)
        {
            var classAvoidance = ClassStats.ClassAvoidance(shipClass);
            var rand = new Random();
            var chance = rand.Next(1, 101);
            if (chance <= classAvoidance)
            {
                var damageTaken = 0;
                return damageTaken;
            }

            var damage = DamagePoint(level);
            return damage;
        }
    }
}