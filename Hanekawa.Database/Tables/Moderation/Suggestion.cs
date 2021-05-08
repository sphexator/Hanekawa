using System;
using Disqord;

namespace Hanekawa.Database.Tables.Moderation
{
    public class Suggestion
    {
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public bool Status { get; set; } = false;
        public Snowflake? MessageId { get; set; }
        public Snowflake? ResponseUser { get; set; }
        public string Response { get; set; } = "No response";
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}