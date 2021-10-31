using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Account
{
    public class Highlight
    {
        public Highlight() { }
        public Highlight(Snowflake guildId, Snowflake userId)
        {
            GuildId = guildId;
            UserId = userId;
        }
        
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public List<string> Keywords { get; set; }
    }
}