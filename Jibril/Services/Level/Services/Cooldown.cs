using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Level.Services
{
    public class Cooldown
    {
        public static Boolean ExperienceCooldown(DateTime cooldown)
        {
            DateTime now = DateTime.Now;

            TimeSpan diff = cooldown.AddSeconds(59) - now;
            Int32 x = Int32.Parse(diff.Seconds.ToString());

            int difference = DateTime.Compare(cooldown, now);
            if((cooldown.ToString() == "0001-01-01 00:00:00") || x <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
