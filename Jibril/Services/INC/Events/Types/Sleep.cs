using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;

namespace Hanekawa.Services.INC.Events.Types
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