using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class LoggingConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? LogJoin { get; set; } = null;
        public string WebhookJoin { get; set; }
        public Snowflake? LogMsg { get; set; } = null;
        public string WebhookMessage { get; set; }
        public Snowflake? LogBan { get; set; } = null;
        public string WebhookBan { get; set; }
        public Snowflake? LogAvi { get; set; } = null;
        public string WebhookAvi { get; set; }
        public Snowflake? LogWarn { get; set; } = null;
        public string WebhookWarn { get; set; }
        public Snowflake? LogAutoMod { get; set; } = null;
        public string WebhookAutoMod { get; set; }
        public Snowflake? LogVoice { get; set; } = null;
        public string WebhookVoice { get; set; }
        public Snowflake? LogReaction { get; set; } = null;
        public string ReactionWebhook { get; set; } = null;
    }
}