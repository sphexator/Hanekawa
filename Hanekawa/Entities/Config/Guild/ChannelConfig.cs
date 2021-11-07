using System.Collections.Generic;
using Hanekawa.Entities.Config.SelfAssign;

namespace Hanekawa.Entities.Config.Guild
{
    public class ChannelConfig
    {
        public ulong GuildId { get; set; }
        public ulong? ReportChannel { get; set; }
        public string WebhookReport { get; set; }

        public ulong? SelfAssignableChannel { get; set; } = null;
        public List<SelfAssignReactionRole> AssignReactionRoles { get; set; } = new ();
    }
}