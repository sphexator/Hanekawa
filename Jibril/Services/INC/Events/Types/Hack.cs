using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;

namespace Hanekawa.Services.INC.Events.Types
{
    public class Hack
    {
        public static string HackEvent(HungerGameLive profile)
        {
            using (var db = new DbService())
            {
                var user = db.HungerGameLives.Find(profile.UserId);

                user.Food = user.Food + 4;
                user.Water = user.Water + 4;
                user.Axe = user.Axe + 1;
                user.Pistol = user.Pistol + 1;
                user.Bow = user.Bow + 1;
                user.TotalWeapons = user.TotalWeapons + 3;
                db.SaveChanges();
                const string response = "Hacked the system, obtaining every single item";
                return response;
            }
        }
    }
}
