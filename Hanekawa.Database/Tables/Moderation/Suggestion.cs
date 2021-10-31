using System;
using Disqord;

namespace Hanekawa.Database.Tables.Moderation
{
    public class Suggestion
    {
        public Suggestion() {}
        public Suggestion(int number, Snowflake guildId, Snowflake userId)
        {
            Id = number;
            GuildId = guildId;
            UserId = userId;
        }
        
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public bool Status { get; set; } = false;
        public Snowflake? MessageId { get; set; } = null;
        public Snowflake? ResponseUser { get; set; } = null;
        public string Response { get; set; } = "No response";
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }
}