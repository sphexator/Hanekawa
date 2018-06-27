namespace Jibril.Services.Entities.Tables
{
    public class NudeServiceChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public uint Tolerance { get; set; }
    }
}