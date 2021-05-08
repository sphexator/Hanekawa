using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class LootChannel
    {
        public Snowflake GuildId { get; set; }
        public Snowflake ChannelId { get; set; }
    }
}