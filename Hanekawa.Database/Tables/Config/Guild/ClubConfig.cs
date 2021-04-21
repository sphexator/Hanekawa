using Disqord;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class ClubConfig
    {
        public Snowflake GuildId { get; set; }
        public Snowflake? ChannelCategory { get; set; } = null;
        public Snowflake? AdvertisementChannel { get; set; } = null;
        public bool EnableTextChannel { get; set; } = false;
        public bool EnableVoiceChannel { get; set; } = false;
        public int ChannelRequiredAmount { get; set; } = 4;
        public int ChannelRequiredLevel { get; set; } = 40;
        public bool AutoPrune { get; set; } = false;
        public bool RoleEnabled { get; set; } = false;
    }
}
