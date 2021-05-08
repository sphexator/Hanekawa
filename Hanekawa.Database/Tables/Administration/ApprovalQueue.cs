using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Administration
{
    public class ApprovalQueue
    {
        // TODO: Add approval queue
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake Uploader { get; set; }
        public string Url { get; set; }
        public ApprovalQueueType Type { get; set; }
        public DateTimeOffset UploadTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    }
}