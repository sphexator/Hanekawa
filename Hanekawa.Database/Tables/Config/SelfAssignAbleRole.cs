using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class SelfAssignAbleRole
    {
        public Snowflake GuildId { get; set; }
        public Snowflake RoleId { get; set; }
        public Snowflake? EmoteId { get; set; }
        public bool Exclusive { get; set; } = false;

        public string EmoteReactFormat { get; set; }
        public string EmoteMessageFormat { get; set; }
    }
}