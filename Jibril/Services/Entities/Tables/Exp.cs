using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Exp
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public uint Tokens { get; set; }
        public uint EventTokens { get; set; }
        public int Level { get; set; }
        public int Xp { get; set; }
        public int TotalXp { get; set; }
        public DateTime? Daily { get; set; }
        public DateTime? Cooldown { get; set; }
        public DateTime? VoiceTimer { get; set; }
        public DateTime? Joindate { get; set; }
        public string ShipClass { get; set; }
        public string FleetName { get; set; }
        public string Profilepic { get; set; }
        public string Hasrole { get; set; }
        public double Toxicityvalue { get; set; }
        public int Toxicitymsgcount { get; set; }
        public double Toxicityavg { get; set; }
        public int MvpCounter { get; set; }
        public int Mvpimmunity { get; set; }
        public int Mvpignore { get; set; }
        public int Rep { get; set; }
        public DateTime Repcd { get; set; }
        public DateTime? Firstmsg { get; set; }
        public DateTime? Lastmsg { get; set; }
    }
}
