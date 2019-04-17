namespace Hanekawa.Database.Tables.Config
{
    public class NudeServiceChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int Tolerance { get; set; } = 80;
        public bool InHouse { get; set; } = false;
    }
}