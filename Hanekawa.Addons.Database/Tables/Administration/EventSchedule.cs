using System;

namespace Hanekawa.Addons.Database.Tables.Administration
{
    public class EventSchedule
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Host { get; set; }
        public ulong? DesignerClaim { get; set; }
        public string Name { get; set; } = "test";
        public string ImageUrl { get; set; } = "test";
        public string Description { get; set; } = "test";
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public bool Posted { get; set; } = false;
    }
}