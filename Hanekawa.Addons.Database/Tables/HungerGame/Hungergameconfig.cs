using System;

namespace Hanekawa.Addons.Database.Tables
{
    public class HungerGameConfig
    {
        public ulong GuildId { get; set; }
        public ulong MessageId { get; set; }
        public bool SignupStage { get; set; }
        public bool Live { get; set; }
        public uint Round { get; set; }
        public DateTime SignupTime { get; set; }
        public int WinCredit { get; set; }
        public int WinSpecialCredit { get; set; }
        public int WinExp { get; set; }
        public ulong? WinnerRoleId { get; set; }
    }
}