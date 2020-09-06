using System;

namespace Hanekawa.Database.Tables.Advertise
{
    public class VoteLog
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        public string Type { get; set; }
    }
}