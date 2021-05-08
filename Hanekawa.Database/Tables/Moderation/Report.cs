using System;
using Disqord;

namespace Hanekawa.Database.Tables.Moderation
{
    public class Report
    {
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public Snowflake? MessageId { get; set; }
        public bool Status { get; set; } = false;
        public string Message { get; set; } = "No message";
        public string Attachment { get; set; } = null;
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}