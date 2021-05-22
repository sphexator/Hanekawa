using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class LoggingConfig
    {
        public Snowflake GuildId { get; set; }
        
        public Snowflake? LogJoin { get; set; }
        public string WebhookJoin { get; set; }
        public Snowflake? WebhookJoinId { get; set; }
        
        public Snowflake? LogMsg { get; set; }
        public string WebhookMessage { get; set; }
        public Snowflake? WebhookMessageId { get; set; }
        
        public Snowflake? LogBan { get; set; }
        public string WebhookBan { get; set; }
        public Snowflake? WebhookBanId { get; set; }
        
        public Snowflake? LogAvi { get; set; }
        public string WebhookAvi { get; set; }
        public Snowflake? WebhookAviId { get; set; }
        
        public Snowflake? LogWarn { get; set; }
        public string WebhookWarn { get; set; }
        public Snowflake? WebhookWarnId { get; set; }
        
        public Snowflake? LogAutoMod { get; set; }
        public string WebhookAutoMod { get; set; }
        public Snowflake? WebhookAutoModId { get; set; }
        
        public Snowflake? LogVoice { get; set; }
        public string WebhookVoice { get; set; }
        public Snowflake? WebhookVoiceId { get; set; }
    }
}