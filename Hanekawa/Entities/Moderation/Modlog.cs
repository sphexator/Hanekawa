using System;

namespace Hanekawa.Entities.Moderation
{
    public class ModLog
    {
        public ModLog() { }

        public ModLog(int id, ulong guildId, ulong userId)
        {
            Id = id;
            GuildId = guildId;
            UserId = userId;
        }
        
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string Action { get; set; } = "Test";
        public ulong MessageId { get; set; }
        public ulong? ModId { get; set; }
        public string Response { get; set; } = "No response";
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}