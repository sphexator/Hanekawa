using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class SuggestionConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? Channel { get; set; } = null;
        public string EmoteYes { get; set; }
        public string EmoteNo { get; set; }
    }
}