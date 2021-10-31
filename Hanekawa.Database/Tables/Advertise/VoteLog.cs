using System;
using Disqord;

namespace Hanekawa.Database.Tables.Advertise
{
    public class VoteLog
    {
        public VoteLog() { }
        public VoteLog(int number, Snowflake guildId, Snowflake userId)
        {
            Id = number;
            GuildId = guildId;
            UserId = userId;
        }
        
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        public string Type { get; set; }
    }
}