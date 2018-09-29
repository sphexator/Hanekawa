using System;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Idle
    {
        private static readonly string[] IdleStrings =
        {
            "Looks at the sky, pondering about life",
            "frozen in time",
            "Standing still, looking at a tree",
            "Wonders if its possible to do ninjutsu"
        };

        public static string IdleEvent()
        {
            var rand = new Random();
            var msg = IdleStrings[rand.Next(0, IdleStrings.Length)];
            return msg;
        }
    }
}