using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using System;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Die
    {
        public static string DieEvent(HungerGameLive profile, DbService db)
        {
            var rand = new Random();
            var response = DieResponseStrings[rand.Next(0, DieResponseStrings.Length)];
            var user = db.HungerGameLives.Find(profile.UserId);
            user.Status = true;
            user.Health = 0;
            db.SaveChanges();
            return response;
        }

        private static readonly string[] DieResponseStrings =
        {
            "Climbed up a tree and fell to his death",
            "Got bit by a snake and decided to chop his leg off, bleed to death",
            "I used to be interested in this game, but then I took an arrow to the knee"
        };
    }
}
