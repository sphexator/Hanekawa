using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Eat
    {
        public static string EatEvent(HungerGameLive profile, DbService db)
        {
            var user = db.HungerGameLives.Find(profile.UserId);
            if (profile.Food == 0) return null;
            user.Hunger = 0;
            user.Thirst = 0;
            user.Fatigue = 0;
            user.Food = user.Food - 1;
            db.SaveChanges();
            return "Ate fish";
        }
    }
}