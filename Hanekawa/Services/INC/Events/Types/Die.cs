using System;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;

namespace Hanekawa.Services.INC.Events.Types
{
    public class Die
    {
        public static string DieEvent(HungerGameLive profile)
        {
            using (var db = new DbService())
            {
                var rand = new Random();
                var response = DieResponseStrings[rand.Next(0, DieResponseStrings.Length)];
                var user = db.HungerGameLives.Find(profile.UserId);
                user.Status = true;
                user.Health = 0;
                db.SaveChanges();
                return response;
            }
        }

        private static readonly string[] DieResponseStrings =
        {
            "Climbed up a tree and fell to his death",
            "Got bit by a snake and decided to chop his leg off, bleed to death",
            "I used to be interested in this game, but then I took an arrow to the knee"
        };
    }
}
