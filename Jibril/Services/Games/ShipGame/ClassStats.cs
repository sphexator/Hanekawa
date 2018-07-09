using Jibril.Data.Variables;

namespace Jibril.Services.Games.ShipGame
{
    public class ClassStats
    {
        public static double ClassDamageModifier(int damage, string shipClass)
        {
            switch (shipClass)
            {
                case ClassNames.Lc:
                {
                    var dmg = damage * 1.1;
                    return dmg;
                }
                case ClassNames.Hc:
                {
                    var dmg = damage * 1.35;
                    return dmg;
                }
                case ClassNames.Dd:
                {
                    var dmg = damage * 2;
                    return dmg;
                }
                case ClassNames.Ac:
                {
                    var dmg = damage * 1.5;
                    return dmg;
                }
                case ClassNames.Bb:
                {
                    var dmg = damage * 1.5;
                    return dmg;
                }
            }

            return damage;
        }

        public static double ClassHealthModifier(int health, string shipClass)
        {
            switch (shipClass)
            {
                case ClassNames.Lc:
                {
                    var hp = health * 1.1;
                    return hp;
                }
                case ClassNames.Hc:
                {
                    var hp = health * 1.35;
                    return hp;
                }
                case ClassNames.Dd:
                {
                    var hp = health / 2.5;
                    return hp;
                }
                case ClassNames.Ac:
                {
                    var hp = health / 3;
                    return hp;
                }
                case ClassNames.Bb:
                {
                    var hp = health * 2;
                    return hp;
                }
            }

            return health;
        }

        public static int ClassAvoidance(string shipClass)
        {
            switch (shipClass)
            {
                case ClassNames.Lc:
                {
                    const int avoidance = 25;
                    return avoidance;
                }
                case ClassNames.Hc:
                {
                    const int avoidance = 20;
                    return avoidance;
                }
                case ClassNames.Dd:
                {
                    const int avoidance = 40;
                    return avoidance;
                }
                case ClassNames.Ac:
                {
                    const int avoidance = 80;
                    return avoidance;
                }
                case ClassNames.Bb:
                {
                    const int avoidance = 15;
                    return avoidance;
                }
                default:
                {
                    const int avoidance = 10;
                    return avoidance;
                }
            }
        }

        public static int ClassCriticalChance(string shipClass)
        {
            switch (shipClass)
            {
                case ClassNames.Lc:
                {
                    const int crit = 25;
                    return crit;
                }
                case ClassNames.Hc:
                {
                    const int crit = 15;
                    return crit;
                }
                case ClassNames.Dd:
                {
                    const int crit = 30;
                    return crit;
                }
                case ClassNames.Ac:
                {
                    const int crit = 10;
                    return crit;
                }
                case ClassNames.Bb:
                {
                    const int crit = 15;
                    return crit;
                }
                default:
                {
                    const int crit = 5;
                    return crit;
                }
            }
        }
    }
}