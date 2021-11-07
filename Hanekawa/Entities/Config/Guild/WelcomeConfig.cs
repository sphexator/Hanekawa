using System;

namespace Hanekawa.Entities.Config.Guild
{
    public class WelcomeConfig
    {
        public ulong GuildId { get; set; }
        public ulong? Channel { get; set; } = null;
        public ulong? WebhookId { get; set; } = null;
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