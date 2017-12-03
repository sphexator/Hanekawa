using Discord;
using System;
using System.Linq;

namespace Jibril.Services.Welcome.Services
{
    public class WelcomeCooldown
    {
        public static Boolean WelcCd(IUser user)
        {
            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            if (userdata == null) return true;

            var now = DateTime.UtcNow.Date;
            var Cooldown = userdata.JoinDateTime;

            var uc = user.CreatedAt.Date;

            var diff = Cooldown.AddHours(24) - now;
            var x = Int32.Parse(diff.Seconds.ToString());
            if ((Cooldown.ToString() == "0001-01-01 00:00:00") || x <= 0 || uc == now)
            {
                return true;
            }

            DatabaseService.UserJoinedDate(user);
            return false;
        }
    }
}
