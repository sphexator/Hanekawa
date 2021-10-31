using System;
using Disqord;

namespace Hanekawa.Database.Tables.Administration
{
    public class Blacklist
    {
        public Blacklist() {}
        public Blacklist(Snowflake guildId) => GuildId = guildId;
        
        public Snowflake GuildId { get; set; }
        public string Reason { get; set; } = "No reason provided";
        public Snowflake ResponsibleUser { get; set; }
        public DateTimeOffset? Unban { get; set; } = DateTimeOffset.UtcNow;
    }
}