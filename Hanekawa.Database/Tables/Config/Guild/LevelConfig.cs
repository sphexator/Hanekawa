namespace Hanekawa.Database.Tables.Config.Guild
{
    public class LevelConfig
    {
        public ulong GuildId { get; set; }
        public double ExpMultiplier { get; set; } = 1;
        public bool VoiceExpEnabled { get; set; } = true;
        public bool StackLvlRoles { get; set; } = true;
    }
}