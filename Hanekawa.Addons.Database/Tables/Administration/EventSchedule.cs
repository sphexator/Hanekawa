using System;

namespace Hanekawa.Addons.Database.Tables.Administration
{
    public class EventSchedule
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Host { get; set; }
        public ulong? DesignerClaim { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
    }
}