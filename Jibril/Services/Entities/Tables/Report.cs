using System;

namespace Jibril.Services.Entities.Tables
{
    public class Report
    {
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public DateTime Date { get; set; }
    }
}
