using System;
using System.Collections.Generic;

namespace Jibril.Services.Entities.Tables
{
    public class Account
    {
        public ulong UserId { get; set; }
        public uint Credit { get; set; }
        public uint CreditSpecial { get; set; }
        public uint Level { get; set; }
        public uint Exp { get; set; }
        public uint TotalExp { get; set; }
        public DateTime VoiceExpTime { get; set; }
        public DateTime DailyCredit { get; set; }
        public string Class { get; set; }
        public string ProfilePic { get; set; }
        public ulong? CustomRoleId { get; set; }
        public uint MvpCounter { get; set; }
        public bool MvpIgnore { get; set; }
        public bool MvpImmunity { get; set; }
        public uint Rep { get; set; }
        public DateTime RepCooldown { get; set; }
        public DateTime LastMessage { get; set; }
        public DateTime? FirstMessage { get; set; }
        public List<Inventory> Inventory { get; set; }
        public uint GameKillAmount { get; set; }
    }
}