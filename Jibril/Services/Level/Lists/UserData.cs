using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Level.Lists
{
    public class UserData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public int Tokens { get; set; }
        public int Event_tokens { get; set; }
        public int Level { get; set; }
        public int Xp { get; set; }
        public int Total_xp { get; set; }
        public DateTime Daily { get; set; }
        public DateTime Cooldown { get; set; }
        public DateTime Voice_timer { get; set; }
        public string FleetName { get; set; }
        public string ShipClass { get; set; }
        public string Profilepic { get; set; }
        public DateTime GameCD { get; set; }
        public DateTime BetCD { get; set; }
        public string Hasrole { get; set; }
    }
}
