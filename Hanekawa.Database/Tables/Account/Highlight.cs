using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Database.Tables.Account
{
    public class Highlight
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public List<string> Keywords { get; set; }
    }
}