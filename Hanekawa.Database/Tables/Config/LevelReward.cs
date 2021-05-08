using Disqord;

namespace Hanekawa.Database.Tables.Config
{
    public class LevelReward
    {
        public Snowflake GuildId { get; set; }
        public int Level { get; set; }
        public Snowflake Role { get; set; }
        public bool Stackable { get; set; } = false;
        public bool NoDecay { get; set; } = false;
    }
}