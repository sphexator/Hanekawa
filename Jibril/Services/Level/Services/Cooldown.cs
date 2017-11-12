using System;

namespace Jibril.Services.Level.Services
{
    public class Cooldown
    {
        public static bool ExperienceCooldown(DateTime cooldown)
        {
            var now = DateTime.Now;

            var diff = cooldown.AddSeconds(59) - now;
            var x = int.Parse(diff.Seconds.ToString());

            var difference = DateTime.Compare(cooldown, now);
            if (cooldown.ToString() == "0001-01-01 00:00:00" || x <= 0)
                return true;
            return false;
        }
    }
}