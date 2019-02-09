using System;

namespace Hanekawa.Addons.Database.Tables.Club
{
    public class ClubInformation
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public ulong GuildId { get; set; }
        public ulong LeaderId { get; set; }
        public string Name { get; set; } = "N/A";
        public string Description { get; set; } = "N/A";
        public string IconUrl { get; set; } = null;
        public string ImageUrl { get; set; } = null;
        public bool Public { get; set; } = false;
        public bool AutoAdd { get; set; } = false;
        public ulong? AdMessage { get; set; } = null;
        public ulong? Channel { get; set; } = null;
        public ulong? Role { get; set; } = null;
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
    }
}