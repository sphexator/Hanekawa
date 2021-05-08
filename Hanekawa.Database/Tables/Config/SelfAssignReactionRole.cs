using System.Collections.Generic;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Database.Tables.Config
{
    public class SelfAssignReactionRole
    {
        public Snowflake GuildId { get; set; }
        public Snowflake ChannelId { get; set; }
        public Snowflake MessageId { get; set; }
        public string Name { get; set; } = "Self-Assignable Roles";
        public List<string> Reactions { get; set; }
        public bool Exclusive { get; set; }
        
        public Snowflake ConfigId { get; set; }
        public ChannelConfig Config { get; set; }
    }
}