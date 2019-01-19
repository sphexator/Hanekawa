using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using System;

namespace Hanekawa.Addons.Database.Extensions
{
    internal static class DefaultConfig
    {
        internal static GuildConfig DefaultGuildConfig(this GuildConfig cfg, ulong guild)
        {
            cfg.GuildId = guild;
            cfg.WelcomeChannel = null;
            cfg.LogMsg = null;
            cfg.LogJoin = null;
            cfg.LogBan = null;
            cfg.LogAvi = null;
            cfg.StackLvlRoles = true;
            cfg.ExpMultiplier = 1;
            cfg.MuteRole = null;
            cfg.WelcomeLimit = 5;
            cfg.Prefix = "h.";
            cfg.BoardChannel = null;
            cfg.IgnoreAllChannels = false;
            cfg.WelcomeBanner = true;
            cfg.WelcomeMessage = null;
            cfg.FilterInvites = false;
            cfg.ReportChannel = null;
            cfg.SuggestionChannel = null;
            cfg.EventChannel = null;
            cfg.MusicVcChannel = null;
            cfg.ModChannel = null;
            cfg.MusicChannel = null;
            cfg.BoardEmote = null;
            cfg.EventSchedulerChannel = null;
            cfg.FilterAllInv = true;
            cfg.FilterMsgLength = null;
            cfg.FilterUrls = false;
            cfg.LogWarn = null;
            cfg.WelcomeDelete = null;
            cfg.Premium = false;
            cfg.SpecialCurrencySign = "$";
            cfg.SpecialCurrencyName = "Special Credit";
            cfg.SpecialEmoteCurrency = false;
            cfg.CurrencyName = "Credit";
            cfg.CurrencySign = "$";
            cfg.EmoteCurrency = false;
            cfg.AnimeAirChannel = null;
            cfg.SuggestionEmoteYes = "<:1yes:403870491749777411>";
            cfg.SuggestionEmoteNo = "<:2no:403870492206825472>";
            cfg.ClubAdvertisementChannel = null;
            cfg.ClubChannelCategory = null;
            cfg.ClubChannelRequiredAmount = 4;
            cfg.ClubChannelRequiredLevel = 40;
            cfg.ClubEnableVoiceChannel = false;
            cfg.LogAutoMod = null;
            cfg.EmoteCountFilter = null;
            cfg.MentionCountFilter = null;
            cfg.QuestionAndAnswerChannel = null;
            cfg.DesignChannel = null;
            cfg.AutomaticEventSchedule = false;
            cfg.ClubAutoPrune = false;
            cfg.EmbedColor = 10181046;
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
