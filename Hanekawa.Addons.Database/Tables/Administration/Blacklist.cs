using System;

namespace Hanekawa.Addons.Database.Tables.Administration
{
    public class Blacklist
    {
        public ulong GuildId { get; set; }
        public string Reason { get; set; }
        public ulong ResponsibleUser { get; set; }
        public DateTimeOffset? Unban { get; set; }
    }
}