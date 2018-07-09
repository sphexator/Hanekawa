using System;

namespace Jibril.Services.Games.ShipGame.Data
{
    public class BaseStats
    {
        public int HealthPoint(int level, string shipClass)
        {
            var baseHealth = 100 + 10 * level;
            var health = ClassStats.ClassHealthModifier(baseHealth, shipClass);
            var fixHealth = Math.Round(health);
            var finalizedHealth = Convert.ToInt32(fixHealth);
            return finalizedHealth;
        }

        public int AttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + 3 * level;
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);

            var rand = new Random();
            var randomDamage = finalizedDamage + rand.Next(-20, 20);
            return randomDamage;
        }

        public int BaseAttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + 3 * level;
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);
            return finalizedDamage;
        }

        public int Avoidance(string shipClass, int level)
        {
            var classAvoidance = ClassStats.ClassAvoidance(shipClass);
            var rand = new Random();
            var chance = rand.Next(1, 101);
            if (chance <= classAvoidance)
            {
                const int damageTaken = 0;
                return damageTaken;
            }

            var damage = AttackPoint(level, shipClass);
            return damage;
        }

        public int CriticalStrike(string shipClass, int level)
        {
            var classCritChance = ClassStats.ClassCriticalChance(shipClass);
            var rand = new Random();
            var chance = rand.Next(1, 101);
            if (chance <= classCritChance)
            {
                var damage = AttackPoint(level, shipClass);
                var critDamage = damage * 2;
                var randomCritDamage = critDamage + rand.Next(-10, 10);
                return randomCritDamage;
            }
            else
            {
                var damage = AttackPoint(level, shipClass);
                return damage;
            }
        }
    }
}