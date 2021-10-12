using System;
using Disqord;

namespace Hanekawa.Database.Tables.SelfAssign
{
    public class SelfAssignItem
    {
        public Snowflake RoleId { get; set; }
        public bool Exclusive { get; set; }

        public Snowflake? EmoteId { get; set; }
        public string Emote { get; set; }
        
        public Guid GroupId { get; set; }
        public SelfAssignGroup Group { get; set; }
    }
}