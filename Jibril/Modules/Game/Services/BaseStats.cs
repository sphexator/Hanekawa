using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class BaseStats
    {
        public static int HealthPoint(int level, string shipClass)
        {
            var baseHealth = 100 + (10 * level);
            var health = ClassStats.ClassHealthModifier(baseHealth, shipClass);
            var fixHealth = Math.Round(health);
            var finalizedHealth = Convert.ToInt32(fixHealth);
            return finalizedHealth;
        }

        public static int AttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + (3 * level);
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);

            Random rand = new Random();
            var randomDamage = finalizedDamage + rand.Next(-20, 20);
            return randomDamage;
        }

        public static int BaseAttackPoint(int level, string shipClass)
        {
            var baseAttackPower = 10 + (3 * level);
            var damage = ClassStats.ClassDamageModifier(baseAttackPower, shipClass);
            var fixDamage = Math.Round(damage);
            var finalizedDamage = Convert.ToInt32(fixDamage);
            return finalizedDamage;
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
                var damage = AttackPoint(level, shipClass);
                return damage;
            }
        }

        public static int CriticalStrike(string shipClass, int level)
        {
            var classCritChance = ClassStats.ClassCriticalChance(shipClass);
            Random rand = new Random();
            int chance = rand.Next(1, 101);
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
