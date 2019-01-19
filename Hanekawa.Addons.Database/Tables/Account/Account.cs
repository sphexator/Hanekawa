using System;

namespace Hanekawa.Addons.Database.Tables.Account
{
    public class Account
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public bool Active { get; set; }

        // Economy
        public int Credit { get; set; }
        public int CreditSpecial { get; set; }

        public DateTime DailyCredit { get; set; }

        // Level
        public int Level { get; set; }
        public int Exp { get; set; }
        public int TotalExp { get; set; }

        public DateTime VoiceExpTime { get; set; }

        // Class Profile Role
        public int Class { get; set; }
        public string ProfilePic { get; set; }
        public ulong? CustomRoleId { get; set; }
        public int Rep { get; set; }
        public DateTime RepCooldown { get; set; }

        public int GameKillAmount { get; set; }

        // Stats
        public DateTime? FirstMessage { get; set; }
        public DateTime LastMessage { get; set; }
        public TimeSpan StatVoiceTime { get; set; }
        public int Sessions { get; set; }
        public ulong StatMessages { get; set; }

        // Board
        public int StarGiven { get; set; }

        public int StarReceived { get; set; }

        // Misc
        public DateTime ChannelVoiceTime { get; set; }
    }
}