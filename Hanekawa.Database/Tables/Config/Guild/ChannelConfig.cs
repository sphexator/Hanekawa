using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class ChannelConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ReportChannel { get; set; } = null;
        public Snowflake? EventChannel { get; set; } = null;
        public Snowflake? EventSchedulerChannel { get; set; } = null;
        public Snowflake? ModChannel { get; set; } = null;
        public Snowflake? DesignChannel { get; set; } = null;
        public Snowflake? QuestionAndAnswerChannel { get; set; } = null;

        public Snowflake? SelfAssignableChannel { get; set; } = null;
        public List<SelfAssignReactionRole> AssignReactionRoles { get; set; } = null;
    }
}