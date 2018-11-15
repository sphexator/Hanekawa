namespace Hanekawa.Addons.Database.Tables.Config
{
    public class GuildInfo
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Type { get; set; }
    }
}