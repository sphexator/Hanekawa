using System;
using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.SelfAssign
{
    public class SelfAssignGroup
    {
        public Guid Id { get; set; }
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelId { get; set; }
        public Snowflake? MessageId { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; } = "";
        
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<SelfAssignItem> Roles { get; set; } = new ();
    }
}