using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Data.Constants;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Extensions
{
    public static class DbExtension
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, SocketGuildUser user)
        {
            var userdata = await context.Accounts.FindAsync(user.Id, user.Guild.Id);
            if (userdata != null) return userdata;
            var data = new Account
            {
                UserId = user.Id,
                GuildId = user.Guild.Id,
                Active = true,
                Class = 1,
                Credit = 0,
                CreditSpecial = 0,
                CustomRoleId = null,
                DailyCredit = DateTime.UtcNow,
                GameKillAmount = 0,
                MvpCounter = 0,
                RepCooldown = DateTime.UtcNow,
                Exp = 0,
                VoiceExpTime = DateTime.UtcNow,
                TotalExp = 0,
                MvpIgnore = false,
                MvpImmunity = false,
                Level = 1,
                Sessions = 0,
                StatVoiceTime = TimeSpan.Zero,
                ChannelVoiceTime = DateTime.UtcNow,
                StatMessages = 0,
                Rep = 0,
                ProfilePic = null,
                StarGiven = 0,
                StarReceived = 0
            };
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user.Id, user.Guild.Id);
        }

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user)
        {
            var userdata = await context.Accounts.FindAsync(user.Id, guild.Id);
            if (userdata != null) return userdata;
            var data = new Account
            {
                UserId = user.Id,
                GuildId = guild.Id,
                Active = true,
                Class = 1,
                Credit = 0,
                CreditSpecial = 0,
                CustomRoleId = null,
                DailyCredit = DateTime.UtcNow,
                GameKillAmount = 0,
                MvpCounter = 0,
                RepCooldown = DateTime.UtcNow,
                Exp = 0,
                VoiceExpTime = DateTime.UtcNow,
                TotalExp = 0,
                MvpIgnore = false,
                MvpImmunity = false,
                Level = 1,
                Sessions = 0,
                StatVoiceTime = TimeSpan.Zero,
                ChannelVoiceTime = DateTime.UtcNow,
                StatMessages = 0,
                Rep = 0,
                ProfilePic = null,
                StarGiven = 0,
                StarReceived = 0
            };
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user.Id, guild.Id);
        }

        public static async Task<Account> GetOrCreateUserData(this DbService context, ulong guild, ulong user)
        {
            var userdata = await context.Accounts.FindAsync(user, guild);
            if (userdata != null) return userdata;
            var data = new Account
            {
                UserId = user,
                GuildId = guild,
                Active = true,
                Class = 1,
                Credit = 0,
                CreditSpecial = 0,
                CustomRoleId = null,
                DailyCredit = DateTime.UtcNow,
                GameKillAmount = 0,
                MvpCounter = 0,
                RepCooldown = DateTime.UtcNow,
                Exp = 0,
                VoiceExpTime = DateTime.UtcNow,
                TotalExp = 0,
                MvpIgnore = false,
                MvpImmunity = false,
                Level = 1,
                Sessions = 0,
                StatVoiceTime = TimeSpan.Zero,
                ChannelVoiceTime = DateTime.UtcNow,
                StatMessages = 0,
                Rep = 0,
                ProfilePic = null,
                StarGiven = 0,
                StarReceived = 0
            };
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user, guild);
        }

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IUser user)
        {
            var userdata = await context.AccountGlobals.FindAsync(user.Id);
            if (userdata != null) return userdata;
            var data = new AccountGlobal
            {
                UserId = user.Id,
                Exp = 0,
                TotalExp = 0,
                Level = 1,
                Rep = 0,
                StarGive = 0,
                StarReceive = 0,
                Credit = 0
            };
            await context.AccountGlobals.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AccountGlobals.FindAsync(user.Id);
        }

        public static async Task<ModLog> CreateCaseId(this DbService context, IUser user, SocketGuild guild,
            DateTime time, ModAction action)
        {
            var counter = await context.ModLogs.CountAsync(x => x.GuildId == guild.Id);
            var data = new ModLog
            {
                Id = (uint)counter + 1,
                GuildId = guild.Id,
                UserId = user.Id,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ModLogs.FirstOrDefaultAsync(x =>
                x.Date == time && x.UserId == user.Id && x.GuildId == guild.Id);
        }

        public static async Task<ClubInfo> CreateClub(this DbService context, IUser user, SocketGuild guild,
            string name, DateTimeOffset time)
        {
            var counter = await context.ClubInfos.CountAsync(x => x.GuildId == guild.Id);
            int nr;
            if (counter == 0) nr = 1;
            else nr = counter + 1;
            var data = new ClubInfo
            {
                Id = nr,
                GuildId = guild.Id,
                Leader = user.Id,
                Name = name,
                CreationDate = time,
                Channel = null,
                Description = null,
                AdMessage = null,
                AutoAdd = false,
                ImageUrl = null,
                Public = false,
                RoleId = null
            };
            await context.ClubInfos.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ClubInfos.FindAsync(nr, guild.Id, user.Id);
        }

        public static async Task<ClubInfo> GetClubAsync(this DbService context, int id, SocketGuild guild)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == guild.Id);
            return check ?? null;
        }

        public static async Task<ClubInfo> IsClubLeader(this DbService context, ulong guild, ulong user)
        {
            var leader = await context.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == guild && x.Leader == user);
            return leader;
        }

        public static async Task<Board> GetOrCreateBoard(this DbService context, IGuild guild, IUserMessage msg)
        {
            var check = await context.Boards.FindAsync(guild.Id, msg.Id);
            if (check != null) return check;
            var data = new Board
            {
                GuildId = guild.Id,
                MessageId = msg.Id,
                StarAmount = 0,
                Boarded = null,
                UserId = msg.Author.Id
            };
            await context.Boards.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Boards.FindAsync(guild.Id, msg.Id);
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.Suggestions.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0) nr = 1;
            else nr = (uint)counter + 1;
            var data = new Suggestion
            {
                Id = nr,
                GuildId = guild.Id,
                Date = time,
                UserId = user.Id,
                Status = true,
            };
            await context.Suggestions.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Suggestions.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<Report> CreateReport(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.Reports.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0) nr = 1;
            else nr = (uint)counter + 1;
            var data = new Report
            {
                Id = nr,
                GuildId = guild.Id,
                UserId = user.Id,
                Status = true,
                Date = time
            };
            await context.Reports.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Reports.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<GuildConfig> GetOrCreateGuildConfig(this DbService context, IGuild guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild.Id);
            if (response != null) return response;
            var data = new GuildConfig
            {
                GuildId = guild.Id,
                WelcomeChannel = null,
                LogMsg = null,
                LogJoin = null,
                LogBan = null,
                LogAvi = null,
                StackLvlRoles = true,
                ExpMultiplier = 1,
                MuteRole = null,
                WelcomeLimit = 5,
                Prefix = "h.",
                BoardChannel = null,
                IgnoreAllChannels = false,
                WelcomeBanner = true,
                WelcomeMessage = null,
                FilterInvites = false,
                ReportChannel = null,
                SuggestionChannel = null,
                EventChannel = null,
                MusicVcChannel = null,
                ModChannel = null,
                MusicChannel = null,
                BoardEmote = null,
                EventSchedulerChannel = null,
                FilterAllInv = true,
                FilterMsgLength = null,
                FilterUrls = false,
                LogWarn = null,
                WelcomeDelete = null,
                Premium = false,
                SpecialCurrencySign = "$",
                SpecialCurrencyName = "Special Credit",
                SpecialEmoteCurrency = false,
                CurrencyName = "Special Credit",
                CurrencySign = "$",
                EmoteCurrency = false,
                AnimeAirChannel = null,
                SuggestionEmoteYes = "<:1yes:403870491749777411>",
                SuggestionEmoteNo = "<:2no:403870492206825472>"
            };
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild.Id);
        }
    }
}