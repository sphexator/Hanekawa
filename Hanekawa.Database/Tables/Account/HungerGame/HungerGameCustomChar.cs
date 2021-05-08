using Disqord;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameCustomChar
    {
        public Snowflake Id { get; set; }
        public Snowflake GuildId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }
}