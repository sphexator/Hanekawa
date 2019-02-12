namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class AdminConfig
    {
        public ulong GuildId { get; set; }
        public ulong? MuteRole { get; set; } = null;
        public bool FilterInvites { get; set; } = false;
        public bool IgnoreAllChannels { get; set; } = false;
        public int? FilterMsgLength { get; set; } = null;
        public bool FilterUrls { get; set; } = false;
        public bool FilterAllInv { get; set; } = false;
        public int? EmoteCountFilter { get; set; } = null;
        public int? MentionCountFilter { get; set; } = null;
    }
}
