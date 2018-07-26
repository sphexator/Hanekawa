namespace Jibril.Services.Entities.Tables
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        // Channel settings
        public ulong? ReportChannel { get; set; }
        public ulong? EventChannel { get; set; }
        public ulong? EventSchedulerChannel { get; set; }
        public ulong? ModChannel { get; set; }
        // Welcome Settings
        public ulong? WelcomeChannel { get; set; }
        public uint WelcomeLimit { get; set; }
        public bool WelcomeBanner { get; set; }
        public string WelcomeMessage { get; set; }
        // Logging settings
        public ulong? LogJoin { get; set; }
        public ulong? LogMsg { get; set; }
        public ulong? LogBan { get; set; }
        public ulong? LogAvi { get; set; }
        // Leveling settings
        public uint ExpMultiplier { get; set; }
        public bool StackLvlRoles { get; set; }
        // Admin settings
        public ulong? MuteRole { get; set; }
        public bool FilterInvites { get; set; }
        public bool IgnoreAllChannels { get; set; }
        // Board settings
        public ulong? BoardEmote { get; set; }
        public ulong? BoardChannel { get; set; }
        // Suggestion settings
        public ulong? SuggestionChannel { get; set; }
        public string SuggestionEmoteYes { get; set; }
        public string SuggestionEmoteNo { get; set; }
        //Music Settings
        public ulong? MusicChannel { get; set; }
        public ulong? MusicVcChannel { get; set; }
    }
}
