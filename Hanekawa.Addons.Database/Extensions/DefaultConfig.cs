using System;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.Config.Guild;

namespace Hanekawa.Addons.Database.Extensions
{
    internal static class DefaultConfig
    {
        internal static GuildConfig DefaultGuildConfig(this GuildConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.Prefix = "h.";
            cfg.MusicVcChannel = null;
            cfg.MusicChannel = null;
            cfg.Premium = false;
            cfg.AnimeAirChannel = null;
            cfg.AutomaticEventSchedule = false;
            cfg.EmbedColor = 10181046;
            return cfg;
        }

        internal static AdminConfig DefaultAdminConfig(this AdminConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.FilterAllInv = true;
            cfg.FilterInvites = false;
            cfg.FilterUrls = false;
            cfg.IgnoreAllChannels = false;
            cfg.MuteRole = null;
            cfg.MentionCountFilter = null;
            cfg.EmoteCountFilter = null;
            cfg.FilterMsgLength = null;
            return cfg;
        }

        internal static BoardConfig DefaultBoardConfig(this BoardConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.Channel = null;
            cfg.Emote = null;
            return cfg;
        }

        internal static ChannelConfig DefaultChannelConfig(this ChannelConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.DesignChannel = null;
            cfg.EventChannel = null;
            cfg.EventSchedulerChannel = null;
            cfg.ModChannel = null;
            cfg.QuestionAndAnswerChannel = null;
            cfg.ReportChannel = null;
            return cfg;
        }

        internal static ClubConfig DefaultClubConfig(this ClubConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.AdvertisementChannel = null;
            cfg.AutoPrune = false;
            cfg.ChannelCategory = null;
            cfg.ChannelRequiredAmount = 4;
            cfg.ChannelRequiredLevel = 10;
            cfg.EnableVoiceChannel = false;
            cfg.RoleEnabled = true;
            return cfg;
        }

        internal static CurrencyConfig DefaultCurrencyConfig(this CurrencyConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.CurrencyName = "Credit";
            cfg.CurrencySign = "$";
            cfg.EmoteCurrency = false;
            cfg.SpecialCurrencyName = "Special credit";
            cfg.SpecialCurrencySign = "$";
            cfg.SpecialEmoteCurrency = false;
            return cfg;
        }

        internal static LevelConfig DefaultLevelConfig(this LevelConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.ExpMultiplier = 1;
            cfg.StackLvlRoles = true;
            return cfg;
        }

        internal static LoggingConfig DefaultLoggingConfig(this LoggingConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.LogMsg = null;
            cfg.LogJoin = null;
            cfg.LogBan = null;
            cfg.LogAvi = null;
            cfg.LogAutoMod = null;
            cfg.LogWarn = null;
            return cfg;
        }

        internal static SuggestionConfig DefaultSuggestionConfig(this SuggestionConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.Channel = null;
            cfg.EmoteYes = "<:1yes:403870491749777411>";
            cfg.EmoteNo = "<:2no:403870492206825472>";
            return cfg;
        }

        internal static WelcomeConfig DefaultWelcomeConfig(this WelcomeConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.Channel = null;
            cfg.Banner = false;
            cfg.Limit = 5;
            cfg.Message = null;
            cfg.TimeToDelete = null;
            cfg.AutoDelOnLeave = false;
            return cfg;
        }

        internal static Account DefaultAccount(this Account account, ulong guild, ulong user)
        {
            account.UserId = user;
            account.GuildId = guild;
            account.Active = true;
            account.Class = 1;
            account.Credit = 0;
            account.CreditSpecial = 0;
            account.DailyCredit = DateTime.UtcNow;
            account.GameKillAmount = 0;
            account.RepCooldown = DateTime.UtcNow;
            account.Exp = 0;
            account.VoiceExpTime = DateTime.UtcNow;
            account.TotalExp = 0;
            account.Level = 1;
            account.Sessions = 0;
            account.StatVoiceTime = TimeSpan.Zero;
            account.ChannelVoiceTime = DateTime.UtcNow;
            account.StatMessages = 0;
            account.Rep = 0;
            account.ProfilePic = null;
            account.StarGiven = 0;
            account.StarReceived = 0;
            return account;
        }

        internal static AccountGlobal DefaultAccountGlobal(this AccountGlobal account, ulong userId)
        {
            account.UserId = userId;
            account.Exp = 0;
            account.TotalExp = 0;
            account.Level = 1;
            account.Rep = 0;
            account.StarGive = 0;
            account.StarReceive = 0;
            account.Credit = 0;
            return account;
        }
    }
}