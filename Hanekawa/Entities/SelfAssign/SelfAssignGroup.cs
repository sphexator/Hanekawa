using System;
using System.Collections.Generic;

namespace Hanekawa.Entities.SelfAssign
{
    public class SelfAssignGroup
    {
        public Guid Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? MessageId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; } = "";
        
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<SelfAssignItem> Roles { get; set; } = new ();
    }
}