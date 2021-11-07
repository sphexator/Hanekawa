namespace Hanekawa.Entities.Config.Guild
{
    public class ReportConfig
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong WebhookId { get; set; }
        public string Webhook { get; set; }
    }
}