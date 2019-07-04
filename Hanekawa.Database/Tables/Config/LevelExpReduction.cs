namespace Hanekawa.Database.Tables.Config
{
    public class LevelExpReduction
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public bool Channel { get; set; }
        public bool Category { get; set; }
        public bool Voice { get; set; }
    }
}