namespace Hanekawa.Entities.Config.Guild
{
    public class LoggingConfig
    {
        public ulong GuildId { get; set; }
        
        public ulong? LogJoin { get; set; }
        public string WebhookJoin { get; set; }
        public ulong? WebhookJoinId { get; set; }
        
        public ulong? LogMsg { get; set; }
        public string WebhookMessage { get; set; }
        public ulong? WebhookMessageId { get; set; }
        
        public ulong? LogBan { get; set; }
        public string WebhookBan { get; set; }
        public ulong? WebhookBanId { get; set; }
        
        public ulong? LogAvi { get; set; }
        public string WebhookAvi { get; set; }
        public ulong? WebhookAviId { get; set; }
        
        public ulong? LogWarn { get; set; }
        public string WebhookWarn { get; set; }
        public ulong? WebhookWarnId { get; set; }
        
        public ulong? LogAutoMod { get; set; }
        public string WebhookAutoMod { get; set; }
        public ulong? WebhookAutoModId { get; set; }
        
        public ulong? LogVoice { get; set; }
        public string WebhookVoice { get; set; }
        public ulong? WebhookVoiceId { get; set; }
    }
}