namespace Hanekawa.Addons.Database.Tables.Config.Guild
{
    public class AdminConfig
    {
        public ulong GuildId { get; set; }
        public ulong? MuteRole { get; set; }
        public bool FilterInvites { get; set; }
        public bool IgnoreAllChannels { get; set; } 
        public int? FilterMsgLength { get; set; }
        public bool FilterUrls { get; set; }
        public bool FilterAllInv { get; set; }
        public int? EmoteCountFilter { get; set; }
        public int? MentionCountFilter { get; set; }
    }
}
