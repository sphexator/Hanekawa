using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Hack
    {
        public static string HackEvent(Hungergame profile)
        {
            using (var db = new hanekawaContext())
            {
                var user = db.Hungergame.Find(profile.Userid.ToString());
                user.Bandages = user.Bandages + 1;
                user.Beans = user.Beans + 1;
                user.Pasta = user.Pasta + 1;
                user.Ramen = user.Ramen + 1;
                user.Fish = user.Fish + 1;
                user.Water = user.Water + 1;
                user.Coke = user.Coke + 1;
                user.Mountaindew = user.Mountaindew + 1;
                user.Redbull = user.Redbull + 1;
                user.Axe = user.Axe + 1;
                user.Pistol = user.Pistol + 1;
                user.Bow = user.Bow + 1;
                user.Totalweapons = user.Totalweapons + 3;
                user.Totaldrink = user.Totaldrink + 4;
                user.Totalfood = user.Totalfood + 4;
                db.SaveChanges();
                const string response = "Hacked the system, obtaining every single item";
                return response;
            }
        }
    }
}
