using System;

namespace Hanekawa.Database.Tables.Club
{
    public class ClubInformation
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong LeaderId { get; set; }
        public string Name { get; set; } = "N/A";
        public string Description { get; set; } = "N/A";
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; } 
        public bool Public { get; set; } 
        public bool AutoAdd { get; set; } 
        public ulong? AdMessage { get; set; } 
        public ulong? Channel { get; set; } 
        public ulong? Role { get; set; } 
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
    }
}