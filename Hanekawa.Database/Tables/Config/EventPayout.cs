using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class EventPayout
    {
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public int Amount { get; set; } = 100;
    }
}