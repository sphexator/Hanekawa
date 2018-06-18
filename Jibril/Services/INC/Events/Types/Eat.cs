using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Eat
    {
        public static string EatEvent(Hungergame profile)
        {
            using (var db = new hanekawaContext())
            {
                var user = db.Hungergame.Find(profile.Userid.ToString());
                if (profile.Fish > 0)
                {
                    user.Hunger = 0;
                    user.Fatigue = 0;
                    user.Fish = user.Fish - 1;
                    db.SaveChanges();
                    return "Ate fish";
                }
                if (profile.Beans > 0)
                {
                    user.Hunger = 0;
                    user.Fatigue = 0;
                    user.Beans = user.Beans - 1;
                    db.SaveChanges();
                    return "Ate Beans";
                }
                if (profile.Pasta > 0)
                {
                    user.Hunger = 0;
                    user.Fatigue = 0;
                    user.Pasta = user.Pasta - 1;
                    db.SaveChanges();
                    return "Ate Pasta";
                }

                if (profile.Ramen <= 0) return null;
                user.Hunger = 0;
                user.Fatigue = 0;
                user.Ramen = user.Ramen - 1;
                db.SaveChanges();
                return "Ate Ramen";
            }
        }
    }
}
