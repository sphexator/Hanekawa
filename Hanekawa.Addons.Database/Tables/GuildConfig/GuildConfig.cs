using System;

namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        public bool Premium { get; set; }
        public int EmbedColor { get; set; }

        // Premium
        public ulong? AnimeAirChannel { get; set; }
        public bool AutomaticEventSchedule { get; set; }

        // Channel settings
        public ulong? ReportChannel { get; set; }
        public ulong? EventChannel { get; set; }
        public ulong? EventSchedulerChannel { get; set; }
        public ulong? ModChannel { get; set; }
        public ulong? DesignChannel { get; set; }
        public ulong? QuestionAndAnswerChannel { get; set; }

        // Club settings
        public ulong? ClubChannelCategory { get; set; }
        public ulong? ClubAdvertisementChannel { get; set; }
        public bool ClubEnableVoiceChannel { get; set; }
        public int ClubChannelRequiredAmount { get; set; }
        public int ClubChannelRequiredLevel { get; set; }
        public bool ClubAutoPrune { get; set; }

        // Welcome Settings
        public ulong? WelcomeChannel { get; set; }
        public int WelcomeLimit { get; set; }
        public bool WelcomeBanner { get; set; }
        public string WelcomeMessage { get; set; }
        public TimeSpan? WelcomeDelete { get; set; }

        // Logging settings
        public ulong? LogJoin { get; set; }
        public ulong? LogMsg { get; set; }
        public ulong? LogBan { get; set; }
        public ulong? LogAvi { get; set; }
        public ulong? LogWarn { get; set; }
        public ulong? LogAutoMod { get;set; }

        // Leveling settings
        public int ExpMultiplier { get; set; }
        public bool StackLvlRoles { get; set; }

        // Currency Settings
        public bool EmoteCurrency { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySign { get; set; }
        public bool SpecialEmoteCurrency { get; set; }
        public string SpecialCurrencyName { get; set; }
        public string SpecialCurrencySign { get; set; }

        // Admin settings
        public ulong? MuteRole { get; set; }
        public bool FilterInvites { get; set; }
        public bool IgnoreAllChannels { get; set; }
        public int? FilterMsgLength { get; set; }
        public bool FilterUrls { get; set; }
        public bool FilterAllInv { get; set; }
        public int? EmoteCountFilter { get; set; }
        public int? MentionCountFilter { get; set; }
        
        // Board settings
        public string BoardEmote { get; set; }
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