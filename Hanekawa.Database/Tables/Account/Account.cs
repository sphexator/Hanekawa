using System;

namespace Hanekawa.Database.Tables.Account
{
    public class Account
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public bool Active { get; set; } = true;

        // Economy
        public int Credit { get; set; } = 0;
        public int CreditSpecial { get; set; } = 0;

        public DateTime DailyCredit { get; set; } = DateTime.UtcNow;

        // Level
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int TotalExp { get; set; } = 0;
        public int Decay { get; set; } = 0;

        public DateTime VoiceExpTime { get; set; } = DateTime.UtcNow;

        // Class Profile Role
        public int Class { get; set; } = 1;
        public string ProfilePic { get; set; } = null;
        public int Rep { get; set; } = 0;
        public DateTime RepCooldown { get; set; } = DateTime.UtcNow;

        public int GameKillAmount { get; set; } = 0;

        // Stats
        public DateTime? FirstMessage { get; set; } = null;
        public DateTime LastMessage { get; set; } = DateTime.UtcNow;
        public TimeSpan StatVoiceTime { get; set; } = TimeSpan.Zero;
        public int Sessions { get; set; } = 0;
        public ulong StatMessages { get; set; } = 0;

        // Board
        public int StarGiven { get; set; } = 0;

        public int StarReceived { get; set; } = 0;

        // Misc
        public DateTime ChannelVoiceTime { get; set; } = DateTime.UtcNow;
        
        // Mvp
        public int MvpCount { get; set; } = 0;
        public bool MvpOptOut { get; set; } = false;
    }
}