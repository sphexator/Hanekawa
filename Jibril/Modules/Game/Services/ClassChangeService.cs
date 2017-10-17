using Discord;
using Jibril.Data.Variables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Game.Services
{
    public class ClassChangeService
    {
        public static int ChangeEligibility(string shipClass, int level)
        {
            if ((shipClass == ClassNames.LC) && (level >= 5))
            {
                return 1;
            }
            else if ((shipClass == ClassNames.HC) && (level >= 10))
            {
                return 1;
            }
            else if ((shipClass == ClassNames.DD) && (level >= 20))
            {
                return 1;
            }
            else if ((shipClass == ClassNames.AC) && (level >= 30))
            {
                return 1;
            }
            else if ((shipClass == ClassNames.BB) && (level >= 40))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static string ShipClass(IUser user, int level)
        {
            if (level >= 2 && level < 5)
            {
                string Shipclass = ClassNames.shipgirl;
                return Shipclass;
            }
            else if (level >= 5 && level < 10)
            {
                string Shipclass = ClassNames.LC;
                return Shipclass;
            }
            else if (level >= 10 && level < 20)
            {
                string Shipclass = ClassNames.HC;
                return Shipclass;
            }
            else if (level >= 20 && level < 30)
            {
                string Shipclass = ClassNames.DD;
                return Shipclass;
            }
            else if (level >= 30 && level < 40)
            {
                string Shipclass = ClassNames.AC;
                return Shipclass;
            }
            else if (level >= 40)
            {
                string Shipclass = ClassNames.BB;
                return Shipclass;
            }
            else
            {
                string Shipclass = ClassNames.shipgirl;
                return Shipclass;
            }
        }
        public static int EligibleClasses(int level)
        {
            if (level >= 2 && level < 5)
            {
                var Classes = 0;
                return Classes;
            }
            else if (level >= 5 && level < 10)
            {
                var Classes = 1;
                return Classes;
            }
            else if (level >= 10 && level < 20)
            {
                var Classes = 2;
                return Classes;
            }
            else if (level >= 20 && level < 30)
            {
                var Classes = 3;
                return Classes;
            }
            else if (level >= 30 && level < 40)
            {
                var Classes = 4;
                return Classes;
            }
            else if (level >= 40)
            {
                var Classes = 5;
                return Classes;
            }
            else
            {
                var Classes = 0;
                return Classes;
            }
        }
    }
}
