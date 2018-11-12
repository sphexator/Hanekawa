namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class LevelExpReduction
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public bool Channel { get; set; }
        public bool Category { get; set; }
    }
}
