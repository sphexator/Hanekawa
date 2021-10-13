using Disqord;

namespace Hanekawa.Database.Tables.Account.HungerGame
{
    public class HungerGameCustomCharacter
    {
        public Snowflake GuildId { get; set; }
        public Snowflake CharacterId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }
}