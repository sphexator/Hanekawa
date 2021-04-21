using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class DropConfig
    {
        public Snowflake GuildId { get; set; }
        public string Emote { get; set; }
        public Snowflake? EmoteId { get; set; }
    }
}
