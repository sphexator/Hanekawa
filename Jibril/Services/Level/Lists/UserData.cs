using System;

namespace Jibril.Services.Level.Lists
{
    public class UserData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public uint Tokens { get; set; }
        public uint Event_tokens { get; set; }
        public int Level { get; set; }
        public int Xp { get; set; }
        public int Total_xp { get; set; }
        public DateTime Daily { get; set; }
        public DateTime Cooldown { get; set; }
        public DateTime Voice_timer { get; set; }
        public DateTime JoinDateTime { get; set; }
        public string FleetName { get; set; }
        public string ShipClass { get; set; }
        public string Profilepic { get; set; }
        public DateTime GameCD { get; set; }
        public DateTime BetCD { get; set; }
        public string Hasrole { get; set; }
        public double Toxicityvalue { get; set; }
        public int Toxicitymsgcount { get; set; }
        public double Toxicityavg { get; set; }
        public int Rep { get; set; }
        public DateTime Repcd { get; set; }
    }
}