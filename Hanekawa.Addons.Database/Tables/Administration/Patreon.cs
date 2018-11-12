using System;

namespace Hanekawa.Addons.Database.Tables.Administration
{
    public class Patreon
    {
        public ulong BotId { get; set; }
        public ulong UserId { get; set; }
        public string Email { get; set; }
        public DateTime Added { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Rewarded { get; set; }
    }
}