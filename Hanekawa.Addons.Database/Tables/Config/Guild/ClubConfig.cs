namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class ClubConfig
    {
        public ulong GuildId { get; set; }
        public ulong? ChannelCategory { get; set; } = null;
        public ulong? AdvertisementChannel { get; set; } = null;
        public bool EnableVoiceChannel { get; set; } = false;
        public int ChannelRequiredAmount { get; set; } = 4;
        public int ChannelRequiredLevel { get; set; } = 40;
        public bool AutoPrune { get; set; } = false;
        public bool RoleEnabled { get; set; } = false;
    }
}
