using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Moderation
{
    public class Warn
    {
        public int Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public WarnReason Type { get; set; } = WarnReason.Warned;
        public string Reason { get; set; } = "No reason provided";
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public ulong Moderator { get; set; }
        public bool Valid { get; set; } = true;
        public TimeSpan? MuteTimer { get; set; } = null;
    }
}