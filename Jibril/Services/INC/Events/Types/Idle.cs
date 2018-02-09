using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.INC.Events.Types
{
    public class Idle
    {
        public static string IdleEvent()
        {
            var rand = new Random();
            var msg = IdleStrings[rand.Next(0, IdleStrings.Length)];
            return msg;
        }

        private static readonly string[] IdleStrings =
        {
            "Looks at the sky, pondering about life",
            "frozen in time",
            "Standing still, looking at a tree",
            "Wonders if its possible to do ninjutsu"
        }; 
    }
}
