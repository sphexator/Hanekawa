using System;

namespace Hanekawa.Services.Entities.Tables
{
    public class EventSchedule
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong Host { get; set;}
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
    }
}