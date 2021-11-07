using System;

namespace Hanekawa.Entities.AutoMessage
{
    public class AutoMessage
    {
        public ulong GuildId { get; set; }
        public ulong Creator { get; set; }
        public ulong ChannelId { get; set; }
        public string Webhook { get; set; }
        public ulong? WebhookId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public TimeSpan Interval { get; set; }
    }
}