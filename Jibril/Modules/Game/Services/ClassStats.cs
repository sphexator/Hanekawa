using Jibril.Data.Variables;

namespace Jibril.Modules.Game.Services
{
    public class ClassStats
    {
        public static double ClassDamageModifier(int damage, string shipClass)
        {
            if (shipClass == ClassNames.Lc)
            {
                var dmg = damage * 1.1;
                return dmg;
            }

            if (shipClass == ClassNames.Hc)
            {
                var dmg = damage * 1.35;
                return dmg;
            }

            if (shipClass == ClassNames.Dd)
            {
                var dmg = damage * 2;
                return dmg;
            }

            if (shipClass == ClassNames.Ac)
            {
                var dmg = damage * 1.5;
                return dmg;
            }

            if (shipClass == ClassNames.Bb)
            {
                var dmg = damage * 1.5;
                return dmg;
            }

            return damage;
        }

        public static double ClassHealthModifier(int health, string shipClass)
        {
            if (shipClass == ClassNames.Lc)
            {
                var hp = health * 1.1;
                return hp;
            }

            if (shipClass == ClassNames.Hc)
            {
                var hp = health * 1.35;
                return hp;
            }

            if (shipClass == ClassNames.Dd)
            {
                var hp = health / 2.5;
                return hp;
            }

            if (shipClass == ClassNames.Ac)
            {
                var hp = health / 3;
                return hp;
            }

            if (shipClass == ClassNames.Bb)
            {
                var hp = health * 2;
                return hp;
            }

            return health;
        }

        public static int ClassAvoidance(string shipClass)
        {
            if (shipClass == ClassNames.Lc)
            {
                var avoidance = 25;
                return avoidance;
            }

            if (shipClass == ClassNames.Hc)
            {
                var avoidance = 20;
                return avoidance;
            }

            if (shipClass == ClassNames.Dd)
            {
                var avoidance = 40;
                return avoidance;
            }

            if (shipClass == ClassNames.Ac)
            {
                var avoidance = 80;
                return avoidance;
            }

            if (shipClass == ClassNames.Bb)
            {
                var avoidance = 15;
                return avoidance;
            }
            else
            {
                var avoidance = 10;
                return avoidance;
            }
        }

        public static int ClassCriticalChance(string shipClass)
        {
            if (shipClass == ClassNames.Lc)
            {
                var crit = 25;
                return crit;
            }

            if (shipClass == ClassNames.Hc)
            {
                var crit = 15;
                return crit;
            }

            if (shipClass == ClassNames.Dd)
            {
                var crit = 30;
                return crit;
            }

            if (shipClass == ClassNames.Ac)
            {
                var crit = 10;
                return crit;
            }

            if (shipClass == ClassNames.Bb)
            {
                var crit = 15;
                return crit;
            }
            else
            {
                var crit = 5;
                return crit;
            }
        }
    }
}