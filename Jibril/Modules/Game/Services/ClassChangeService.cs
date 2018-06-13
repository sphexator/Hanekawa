using Discord;
using Jibril.Data.Variables;

namespace Jibril.Modules.Game.Services
{
    public class ClassChangeService
    {
        public static int ChangeEligibility(string shipClass, int level)
        {
            if (shipClass == ClassNames.Lc && level >= 5)
                return 1;
            if (shipClass == ClassNames.Hc && level >= 10)
                return 1;
            if (shipClass == ClassNames.Dd && level >= 20)
                return 1;
            if (shipClass == ClassNames.Ac && level >= 30)
                return 1;
            if (shipClass == ClassNames.Bb && level >= 40)
                return 1;
            return 0;
        }

        public static string ShipClass(IUser user, int level)
        {
            if (level >= 2 && level < 5)
            {
                var Shipclass = ClassNames.Shipgirl;
                return Shipclass;
            }

            if (level >= 5 && level < 10)
            {
                var Shipclass = ClassNames.Lc;
                return Shipclass;
            }

            if (level >= 10 && level < 20)
            {
                var Shipclass = ClassNames.Hc;
                return Shipclass;
            }

            if (level >= 20 && level < 30)
            {
                var Shipclass = ClassNames.Dd;
                return Shipclass;
            }

            if (level >= 30 && level < 40)
            {
                var Shipclass = ClassNames.Ac;
                return Shipclass;
            }

            if (level >= 40)
            {
                var Shipclass = ClassNames.Bb;
                return Shipclass;
            }
            else
            {
                var Shipclass = ClassNames.Shipgirl;
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