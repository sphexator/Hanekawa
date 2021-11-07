using System.Collections.Generic;
using Hanekawa.Entities.Config.Guild;

namespace Hanekawa.Entities.Config.SelfAssign
{
    public class SelfAssignReactionRole
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Name { get; set; } = "Self-Assignable Roles";
        public List<string> Reactions { get; set; }
        public bool Exclusive { get; set; }
        
        public ulong ConfigId { get; set; }
        public ChannelConfig Config { get; set; }
    }
}