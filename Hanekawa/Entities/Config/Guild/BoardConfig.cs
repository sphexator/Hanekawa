namespace Hanekawa.Entities.Config.Guild
{
    public class BoardConfig
    {
        public ulong GuildId { get; set; }
        public string Emote { get; set; }
        public ulong? Channel { get; set; } = null;
        public ulong? WebhookId { get; set; } = null;
        public string Webhook { get; set; }
    }
}
