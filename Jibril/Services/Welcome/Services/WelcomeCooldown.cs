using Discord;
using System;
using System.Linq;

namespace Jibril.Services.Welcome.Services
{
    public class WelcomeCooldown
    {
        public static bool WelcomeFilter(IUser user)
        {
            var now = DateTime.UtcNow.Date;
            var ucd = user.CreatedAt.Date;
            var compare = DateTime.Compare(now, ucd);
            if (compare > 0) return true;
            return false;
        }

        public static bool Testwelc(IUser user)
        {
            var userdata = DatabaseService.UserData(user).First();
            if (userdata == null) return true;
            var Cooldown = userdata.JoinDateTime;

            var now = DateTime.UtcNow.Date;
            var uc = user.CreatedAt.Date;
            DateTime.Compare(now, uc);
            var difference = DateTime.Compare(Cooldown, now);
            var diff = Cooldown.AddHours(24) - now;
            var x = Int32.Parse(diff.Seconds.ToString());
            if ((Cooldown.ToString() == "0001-01-01 00:00:00") || difference <= 0 || uc == now)
            {
                return true;
            }

            DatabaseService.UserJoinedDate(user);
            return false;
        }
    }
}
