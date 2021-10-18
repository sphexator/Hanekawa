using System;
using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Club
{
    public class Club
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Snowflake GuildId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "N/A";
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }
        public bool Public { get; set; } = false;
        public bool AutoAdd { get; set; } = false;
        public Snowflake? AdMessage { get; set; }
        public Snowflake? Channel { get; set; }
        public Snowflake? Role { get; set; } 
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
        
        public List<ClubUser> Users { get; set; }
        public List<ClubBlacklist> Blacklist { get; set; }
    }
}