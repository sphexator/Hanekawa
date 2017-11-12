using Jibril.Data.Variables;

namespace Jibril.Modules.Game.Services
{
    public class ClassStats
    {
        public static double ClassDamageModifier(int damage, string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var dmg = damage * 1.1;
                return dmg;
            }
            if (shipClass == ClassNames.HC)
            {
                var dmg = damage * 1.35;
                return dmg;
            }
            if (shipClass == ClassNames.DD)
            {
                var dmg = damage * 2;
                return dmg;
            }
            if (shipClass == ClassNames.AC)
            {
                var dmg = damage * 1.5;
                return dmg;
            }
            if (shipClass == ClassNames.BB)
            {
                var dmg = damage * 1.5;
                return dmg;
            }
            return damage;
        }

        public static double ClassHealthModifier(int health, string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var hp = health * 1.1;
                return hp;
            }
            if (shipClass == ClassNames.HC)
            {
                var hp = health * 1.35;
                return hp;
            }
            if (shipClass == ClassNames.DD)
            {
                var hp = health / 2.5;
                return hp;
            }
            if (shipClass == ClassNames.AC)
            {
                var hp = health / 3;
                return hp;
            }
            if (shipClass == ClassNames.BB)
            {
                var hp = health * 2;
                return hp;
            }
            return health;
        }

        public static int ClassAvoidance(string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var avoidance = 25;
                return avoidance;
            }
            if (shipClass == ClassNames.HC)
            {
                var avoidance = 20;
                return avoidance;
            }
            if (shipClass == ClassNames.DD)
            {
                var avoidance = 40;
                return avoidance;
            }
            if (shipClass == ClassNames.AC)
            {
                var avoidance = 80;
                return avoidance;
            }
            if (shipClass == ClassNames.BB)
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
            if (shipClass == ClassNames.LC)
            {
                var crit = 25;
                return crit;
            }
            if (shipClass == ClassNames.HC)
            {
                var crit = 15;
                return crit;
            }
            if (shipClass == ClassNames.DD)
            {
                var crit = 30;
                return crit;
            }
            if (shipClass == ClassNames.AC)
            {
                var crit = 10;
                return crit;
            }
            if (shipClass == ClassNames.BB)
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