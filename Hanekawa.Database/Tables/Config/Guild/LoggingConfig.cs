using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class LoggingConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? LogJoin { get; set; } = null;
        public Snowflake? LogMsg { get; set; } = null;
        public Snowflake? LogBan { get; set; } = null;
        public Snowflake? LogAvi { get; set; } = null;
        public Snowflake? LogWarn { get; set; } = null;
        public Snowflake? LogAutoMod { get; set; } = null;
        public Snowflake? LogVoice { get; set; } = null;
        public Snowflake? LogReaction { get; set; } = null;
        public string ReactionWebhook { get; set; } = null;
    }
}