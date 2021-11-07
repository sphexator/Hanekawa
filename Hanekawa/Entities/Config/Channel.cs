namespace Hanekawa.Entities.Config
{
    public class Channel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ChannelType Type { get; set; }
        public ChannelCategory Category { get; set; }
    }
}