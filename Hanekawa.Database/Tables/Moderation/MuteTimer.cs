using System;
using Disqord;

namespace Hanekawa.Database.Tables.Moderation
{
    public class MuteTimer
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}