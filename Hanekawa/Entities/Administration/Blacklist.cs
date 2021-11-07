using System;

namespace Hanekawa.Entities.Administration
{
    public class Blacklist
    {
        public Blacklist() {}
        public Blacklist(ulong guildId) => GuildId = guildId;
        
        public ulong GuildId { get; set; }
        public string Reason { get; set; } = "No reason provided";
        public ulong ResponsibleUser { get; set; }
        public DateTimeOffset? Unban { get; set; } = DateTimeOffset.UtcNow;
    }
}