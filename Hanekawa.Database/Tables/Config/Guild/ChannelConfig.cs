using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class ChannelConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ReportChannel { get; set; }
        public string WebhookReport { get; set; }

        public Snowflake? SelfAssignableChannel { get; set; } = null;
        public List<SelfAssignReactionRole> AssignReactionRoles { get; set; } = new ();
    }
}