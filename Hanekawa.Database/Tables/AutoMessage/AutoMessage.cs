using System;
using Disqord;

namespace Hanekawa.Database.Tables.AutoMessage
{
    public class AutoMessage
    {
        public Snowflake GuildId { get; set; }
        public Snowflake Creator { get; set; }
        public Snowflake ChannelId { get; set; }
        public string Webhook { get; set; }
        public Snowflake? WebhookId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public TimeSpan Interval { get; set; }
    }
}