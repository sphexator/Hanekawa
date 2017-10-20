using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Gambling.Services
{
    public class BetAdjust
    {
        public static int Adjust (int bet)
        {
            if (bet > 500) return 500;
            else return bet;
        }
    }
}
