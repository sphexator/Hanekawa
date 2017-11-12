using Discord;
using Jibril.Data.Variables;

namespace Jibril.Modules.Game.Services
{
    public class ClassChangeService
    {
        public static int ChangeEligibility(string shipClass, int level)
        {
            if (shipClass == ClassNames.LC && level >= 5)
                return 1;
            if (shipClass == ClassNames.HC && level >= 10)
                return 1;
            if (shipClass == ClassNames.DD && level >= 20)
                return 1;
            if (shipClass == ClassNames.AC && level >= 30)
                return 1;
            if (shipClass == ClassNames.BB && level >= 40)
                return 1;
            return 0;
        }

        public static string ShipClass(IUser user, int level)
        {
            if (level >= 2 && level < 5)
            {
                var Shipclass = ClassNames.shipgirl;
                return Shipclass;
            }
            if (level >= 5 && level < 10)
            {
                var Shipclass = ClassNames.LC;
                return Shipclass;
            }
            if (level >= 10 && level < 20)
            {
                var Shipclass = ClassNames.HC;
                return Shipclass;
            }
            if (level >= 20 && level < 30)
            {
                var Shipclass = ClassNames.DD;
                return Shipclass;
            }
            if (level >= 30 && level < 40)
            {
                var Shipclass = ClassNames.AC;
                return Shipclass;
            }
            if (level >= 40)
            {
                var Shipclass = ClassNames.BB;
                return Shipclass;
            }
            else
            {
                var Shipclass = ClassNames.shipgirl;
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
            if (level >= 5 && level < 10)
            {
                var Classes = 1;
                return Classes;
            }
            if (level >= 10 && level < 20)
            {
                var Classes = 2;
                return Classes;
            }
            if (level >= 20 && level < 30)
            {
                var Classes = 3;
                return Classes;
            }
            if (level >= 30 && level < 40)
            {
                var Classes = 4;
                return Classes;
            }
            if (level >= 40)
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