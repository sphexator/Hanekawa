using System;
using Disqord;

namespace Hanekawa.Database.Tables.Moderation
{
    public class ModLog
    {
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public string Action { get; set; } = "Test";
        public Snowflake MessageId { get; set; }
        public Snowflake? ModId { get; set; }
        public string Response { get; set; } = "No response";
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}