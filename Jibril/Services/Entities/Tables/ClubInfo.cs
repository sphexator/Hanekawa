using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class ClubInfo
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Leader { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public ulong? Channel { get; set; }
        public ulong? RoleId { get; set; }
        public bool Public { get; set; }
        public bool AutoAdd { get; set; }
        public ulong? AdMessage { get; set; }
    }
}
