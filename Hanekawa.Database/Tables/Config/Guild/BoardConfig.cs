using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class BoardConfig
    {
        public Snowflake GuildId { get; set; }
        public string Emote { get; set; }
        public Snowflake? Channel { get; set; } = null;
    }
}
