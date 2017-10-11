using Jibril.Data.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class ClassStats
    {
        public static Double ClassDamageModifier(int damage, string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var dmg = damage * 1.1;
                return dmg;
            }
            else if (shipClass == ClassNames.HC)
            {
                var dmg = damage * 1.35;
                return dmg;
            }
            else if (shipClass == ClassNames.DD)
            {
                var dmg = damage * 2;
                return dmg;
            }
            else if (shipClass == ClassNames.AC)
            {
                var dmg = damage * 1.5;
                return dmg;
            }
            else if (shipClass == ClassNames.BB)
            {
                var dmg = damage * 1.5;
                return dmg;
            }
            else
            {
                return damage;
            }
        }

        public static Double ClassHealthModifier(int health, string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var hp = health * 1.1;
                return hp;
            }
            else if (shipClass == ClassNames.HC)
            {
                var hp = health * 1.35;
                return hp;
            }
            else if (shipClass == ClassNames.DD)
            {
                var hp = health / 2.5;
                return hp;
            }
            else if (shipClass == ClassNames.AC)
            {
                var hp = health / 3;
                return hp;
            }
            else if (shipClass == ClassNames.BB)
            {
                var hp = health * 2;
                return hp;
            }
            else
            {
                return health;
            }
        }

        public static int ClassAvoidance(string shipClass)
        {
            if (shipClass == ClassNames.LC)
            {
                var avoidance = 25;
                return avoidance;
            }
            else if (shipClass == ClassNames.HC)
            {
                var avoidance = 20;
                return avoidance;
            }
            else if (shipClass == ClassNames.DD)
            {
                var avoidance = 40;
                return avoidance;
            }
            else if (shipClass == ClassNames.AC)
            {
                var avoidance = 80;
                return avoidance;
            }
            else if (shipClass == ClassNames.BB)
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
            else if (shipClass == ClassNames.HC)
            {
                var crit = 15;
                return crit;
            }
            else if (shipClass == ClassNames.DD)
            {
                var crit = 30;
                return crit;
            }
            else if (shipClass == ClassNames.AC)
            {
                var crit = 10;
                return crit;
            }
            else if (shipClass == ClassNames.BB)
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
