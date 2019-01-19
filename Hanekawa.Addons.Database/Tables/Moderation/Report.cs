using System;

namespace Hanekawa.Addons.Database.Tables.Moderation
{
    public class Report
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong? MessageId { get; set; }
        public bool Status { get; set; } = false;
        public string Message { get; set; } = "No message";
        public string Attachment { get; set; } = null;
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}