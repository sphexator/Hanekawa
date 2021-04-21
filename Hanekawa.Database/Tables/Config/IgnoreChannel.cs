using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class IgnoreChannel
    {
        public Snowflake GuildId { get; set; }
        public Snowflake ChannelId { get; set; }
    }
}