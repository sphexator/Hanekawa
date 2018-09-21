using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class ClubBlacklist
    {
        public int ClubId { get; set; }
        public ulong GuildId { get; set; }
        public ulong BlackListUser { get; set; }
        public ulong IssuedUser { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
