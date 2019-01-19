using System;

namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; } = "h.";
        public bool Premium { get; set; } = false;
        public int EmbedColor { get; set; }

        // Premium
        public ulong? AnimeAirChannel { get; set; } = null;
        public bool AutomaticEventSchedule { get; set; } = false;

        // Channel settings
        public ulong? ReportChannel { get; set; } = null;
        public ulong? EventChannel { get; set; } = null;
        public ulong? EventSchedulerChannel { get; set; } = null;
        public ulong? ModChannel { get; set; } = null;
        public ulong? DesignChannel { get; set; } = null;
        public ulong? QuestionAndAnswerChannel { get; set; } = null;

        // Club settings
        public ulong? ClubChannelCategory { get; set; } = null;
        public ulong? ClubAdvertisementChannel { get; set; } = null;
        public bool ClubEnableVoiceChannel { get; set; } = false;
        public int ClubChannelRequiredAmount { get; set; } = 4;
        public int ClubChannelRequiredLevel { get; set; } = 40;
        public bool ClubAutoPrune { get; set; } = false;

        // Welcome Settings
        public ulong? WelcomeChannel { get; set; } = null;
        public int WelcomeLimit { get; set; } = 4;
        public bool WelcomeBanner { get; set; } = false;
        public string WelcomeMessage { get; set; } = null;
        public TimeSpan? WelcomeDelete { get; set; } = null;

        // Logging settings
        public ulong? LogJoin { get; set; } = null;
        public ulong? LogMsg { get; set; } = null;
        public ulong? LogBan { get; set; } = null;
        public ulong? LogAvi { get; set; } = null;
        public ulong? LogWarn { get; set; } = null;
        public ulong? LogAutoMod { get; set; } = null;

        // Leveling settings
        public int ExpMultiplier { get; set; } = 1;
        public bool StackLvlRoles { get; set; } = true;

        // Currency Settings
        public bool EmoteCurrency { get; set; } = false;
        public string CurrencyName { get; set; } = "Credit";
        public string CurrencySign { get; set; } = "$";
        public bool SpecialEmoteCurrency { get; set; } = false;
        public string SpecialCurrencyName { get; set; } = "Special credit";
        public string SpecialCurrencySign { get; set; } = "$";

        // Admin settings
        public ulong? MuteRole { get; set; } = null;
        public bool FilterInvites { get; set; } = false;
        public bool IgnoreAllChannels { get; set; } = false;
        public int? FilterMsgLength { get; set; } = null;
        public bool FilterUrls { get; set; } = false;
        public bool FilterAllInv { get; set; } = false;
        public int? EmoteCountFilter { get; set; } = null;
        public int? MentionCountFilter { get; set; } = null;
        
        // Board settings
        public string BoardEmote { get; set; }
        public ulong? BoardChannel { get; set; } = null;

        // Suggestion settings
        public ulong? SuggestionChannel { get; set; } = null;
        public string SuggestionEmoteYes { get; set; }
        public string SuggestionEmoteNo { get; set; }

        //Music Settings
        public ulong? MusicChannel { get; set; } = null;
        public ulong? MusicVcChannel { get; set; } = null;
    }
}