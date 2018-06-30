using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Sleep
    {
        public static string SleepEvent(HungerGameLive profile)
        {
            using (var db = new DbService())
            {
                var user = db.HungerGameLives.Find(profile.UserId);
                user.Sleep = 0;
                user.Fatigue = 0;
                db.SaveChanges();
                return "Fell asleep";
            }
        }
    }
}