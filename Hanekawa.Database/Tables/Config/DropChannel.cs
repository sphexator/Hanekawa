using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class DropChannel
    {
        public Snowflake GuildId { get; set; }
        public Snowflake ChannelId { get; set; }
    }
}