using System;
using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class WelcomeConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? Channel { get; set; } = null;
        public string Webhook { get; set; }
        public int Limit { get; set; } = 4;
        public bool Banner { get; set; } = false;
        public string Message { get; set; }
        public int? Reward { get; set; } = null;
        public TimeSpan? TimeToDelete { get; set; } = null;
        public bool AutoDelOnLeave { get; set; } = false;
        public TimeSpan? IgnoreNew { get; set; } = null;
    }
}