using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Sleep
    {
        public static string SleepEvent(HungerGameLive profile, DbService db)
        {
            var user = db.HungerGameLives.Find(profile.UserId);
            user.Sleep = 0;
            user.Fatigue = 0;
            return "Fell asleep";
        }
    }
}