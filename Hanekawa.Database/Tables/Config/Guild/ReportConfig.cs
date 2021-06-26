using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class ReportConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelId { get; set; }
        public Snowflake WebhookId { get; set; }
        public string Webhook { get; set; }
    }
}