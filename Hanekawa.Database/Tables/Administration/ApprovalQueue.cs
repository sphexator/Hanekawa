using System;
using Hanekawa.Shared;

namespace Hanekawa.Database.Tables.Administration
{
    public class ApprovalQueue
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Uploader { get; set; }
        public string Url { get; set; }
        public ApprovalQueueType Type { get; set; }
        public DateTimeOffset UploadTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    }
}