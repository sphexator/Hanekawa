using System;

namespace Hanekawa.Database.Tables.Administration
{
    public class Blacklist
    {
        public ulong GuildId { get; set; }
        public string Reason { get; set; } = "No reason provided";
        public ulong ResponsibleUser { get; set; }
        public DateTimeOffset? Unban { get; set; } = DateTimeOffset.UtcNow;
    }
}