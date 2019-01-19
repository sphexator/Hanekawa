using System;

namespace Hanekawa.Addons.Database.Tables.Administration
{
    public class Patreon
    {
        public ulong BotId { get; set; }
        public ulong UserId { get; set; }
        public string Email { get; set; } = "test@test.com";
        public DateTime Added { get; set; } = DateTime.UtcNow;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Rewarded { get; set; } = null;
    }
}