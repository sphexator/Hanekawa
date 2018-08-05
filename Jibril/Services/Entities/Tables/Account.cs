using System;
using System.Collections.Generic;

namespace Jibril.Services.Entities.Tables
{
    public class Account
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool Active { get; set; }
        // Economy
        public uint Credit { get; set; }
        public uint CreditSpecial { get; set; }
        public DateTime DailyCredit { get; set; }
        // Level
        public uint Level { get; set; }
        public uint Exp { get; set; }
        public uint TotalExp { get; set; }
        public DateTime VoiceExpTime { get; set; }
        // Class Profile Role
        public int Class { get; set; }
        public string ProfilePic { get; set; }
        public ulong? CustomRoleId { get; set; }
        public uint Rep { get; set; }
        public DateTime RepCooldown { get; set; }
        public uint GameKillAmount { get; set; }
        // MVP
        public uint MvpCounter { get; set; }
        public bool MvpIgnore { get; set; }
        public bool MvpImmunity { get; set; }
        // Stats
        public DateTime? FirstMessage { get; set; }
        public DateTime LastMessage { get; set; }
        public TimeSpan StatVoiceTime { get; set; }
        public uint Sessions { get; set; }
        public ulong StatMessages { get; set; }
        // Board
        public uint StarGiven { get; set; }
        public uint StarReceived { get; set; }
        // Misc
        /// <summary> Duration a user has been in one channel </summary>
        public DateTime ChannelVoiceTime { get; set; }
    }
}