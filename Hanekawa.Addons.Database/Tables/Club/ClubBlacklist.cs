using System;

namespace Hanekawa.Addons.Database.Tables.Club
{
    public class ClubBlacklist
    {
        public int ClubId { get; set; }
        public ulong GuildId { get; set; }
        public ulong BlackListUser { get; set; }
        public ulong IssuedUser { get; set; }
        public string Reason { get; set; } = "No reason provided";
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
    }
}