using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Sleep
    {
        public static string SleepEvent(Hungergame profile)
        {
            using (var db = new hanekawaContext())
            {
                var user = db.Hungergame.Find(profile.Userid);
                user.Sleep = 0;
                user.Fatigue = 0;
                db.SaveChanges();
                return "Fell asleep";
            }
        }
    }
}