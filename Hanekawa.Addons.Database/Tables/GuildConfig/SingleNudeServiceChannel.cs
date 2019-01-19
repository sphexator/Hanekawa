namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class SingleNudeServiceChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int? Level { get; set; }
        public int? Tolerance { get; set; } = 80;
        public bool InHouse { get; set; }
    }
}
