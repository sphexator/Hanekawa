using System;

namespace Hanekawa.Database.Tables.Advertise
{
    public class DblAuth
    {
        public ulong GuildId { get; set; }
        public Guid AuthKey { get; set; }

        public int ExpGain { get; set; } = 0;
        public int CreditGain { get; set; } = 0;
        public int SpecialCredit { get; set; } = 0;

        public ulong? RoleIdReward { get; set; } = null;
        public string Message { get; set; } = null;
    }
}