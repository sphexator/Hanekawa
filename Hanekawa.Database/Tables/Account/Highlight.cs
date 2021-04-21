using Disqord;

namespace Hanekawa.Database.Tables.Account
{
    public class Highlight
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public string[] Highlights { get; set; }
    }
}