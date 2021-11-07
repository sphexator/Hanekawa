using System;

namespace Hanekawa.Entities.SelfAssign
{
    public class SelfAssignItem
    {
        public ulong RoleId { get; set; }
        public bool Exclusive { get; set; }

        public ulong? EmoteId { get; set; }
        public string Emote { get; set; }
        
        public Guid GroupId { get; set; }
        public SelfAssignGroup Group { get; set; }
    }
}