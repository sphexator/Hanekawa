using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.HungerGames.Data;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Eat
    {
        public static string EatEvent(HungerGameLive profile)
        {
            using (var db = new DbService())
            {
                var user = db.HungerGameLives.Find(profile.UserId);
                if (profile.Food <= 0) return null;
                user.Hunger = 0;
                user.Fatigue = 0;
                user.Food = user.Food - 1;
                db.SaveChanges();
                return "Ate fish";

            }
        }
    }
}
