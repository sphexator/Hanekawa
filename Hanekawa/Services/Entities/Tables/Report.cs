using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class Report
    {
        public uint Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong? MessageId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public DateTime Date { get; set; }
    }
}
