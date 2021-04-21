using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class LevelConfig
    {
        public Snowflake GuildId { get; set; }
        public double TextExpMultiplier { get; set; } = 1;
        public double VoiceExpMultiplier { get; set; } = 1;
        public double BoostExpMultiplier { get; set; } = 1;
        public bool Decay { get; set; } = false;
        public bool ExpDisabled { get; set; } = false;
        public bool VoiceExpEnabled { get; set; } = true;
        public bool TextExpEnabled { get; set; } = true;
        public bool StackLvlRoles { get; set; } = true;
    }
}