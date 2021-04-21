using Disqord;

namespace Hanekawa.Database.Tables
{
    public class VoiceRoles
    {
        public Snowflake GuildId { get; set; }
        public Snowflake VoiceId { get; set; }
        public Snowflake RoleId { get; set; }
    }
}