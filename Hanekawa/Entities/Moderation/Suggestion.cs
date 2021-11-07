using System;

namespace Hanekawa.Entities.Moderation
{
    public class Suggestion
    {
        public Suggestion() {}
        public Suggestion(int number, ulong guildId, ulong userId)
        {
            Id = number;
            GuildId = guildId;
            UserId = userId;
        }
        
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool Status { get; set; } = false;
        public ulong? MessageId { get; set; } = null;
        public ulong? ResponseUser { get; set; } = null;
        public string Response { get; set; } = "No response";
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }
}