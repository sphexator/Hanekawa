using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public class Die
    {
        public static string DieEvent(Hungergame profile)
        {
            using (var db = new hanekawaContext())
            {
                var rand = new Random();
                var response = DieResponseStrings[rand.Next(0, DieResponseStrings.Length)];
                var user = db.Hungergame.Find(profile.Userid.ToString());
                user.Status = true;
                user.DamageTaken = 100;
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
