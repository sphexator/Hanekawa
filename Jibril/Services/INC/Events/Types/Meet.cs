using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.INC.Events.Types
{
    public class Meet
    {
        public static string MeetEvent()
        {
            var rand = new Random();
            var response = MeetEventStrings[rand.Next(0, MeetEventStrings.Length)];
            return response;
        }

        private static readonly string[] MeetEventStrings =
        {
            "Climbed up in a tree, seeing someone in the distance",
            "Lurks behind a tree, spying on someone"
        };
    }
}
