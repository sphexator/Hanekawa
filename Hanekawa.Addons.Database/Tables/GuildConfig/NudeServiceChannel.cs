namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class NudeServiceChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public uint Tolerance { get; set; }
    }
}