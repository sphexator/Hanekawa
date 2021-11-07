using System;

namespace Hanekawa.Entities.Advertise
{
    public class VoteLog
    {
        public VoteLog() { }
        public VoteLog(int number, ulong guildId, ulong userId)
        {
            Id = number;
            GuildId = guildId;
            UserId = userId;
        }
        
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        public string Type { get; set; }
    }
}