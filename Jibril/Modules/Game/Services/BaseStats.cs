using System;

namespace Jibril.Modules.Game.Services
{
    public class BaseStats
    {
        public static int HealthPoint(int level, string shipClass)
        {
            var baseHealth = 100 + 10 * level;
            var health = ClassStats.ClassHealthModifier(baseHealth, shipClass);
            var fixHealth = Math.Round(health);
            var finalizedHealth = Convert.ToInt32(fixHealth);
            return finalizedHealth;
        }

        public static int AttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + 3 * level;
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);

            var rand = new Random();
            var randomDamage = finalizedDamage + rand.Next(-20, 20);
            return randomDamage;
        }

        public static int BaseAttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + 3 * level;
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);
            return finalizedDamage;
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

            var damage = AttackPoint(level, shipClass);
            return damage;
        }

        public static int CriticalStrike(string shipClass, int level)
        {
            var classCritChance = ClassStats.ClassCriticalChance(shipClass);
            var rand = new Random();
            var chance = rand.Next(1, 101);
            if (chance <= classCritChance)
            {
                var Damage = AttackPoint(level, shipClass);
                var critDamage = Damage * 2;
                var randomCritDamage = critDamage + rand.Next(-10, 10);
                return randomCritDamage;
            }
            else
            {
                var Damage = AttackPoint(level, shipClass);
                return Damage;
            }
        }
    }
}