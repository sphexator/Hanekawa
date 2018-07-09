using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Data.Constants;
using Jibril.Data.Variables;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Extensions
{
    public static class DbExtension
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, IUser user)
        {
            var userdata = await context.Accounts.FindAsync(user.Id);
            if (userdata != null) return userdata;
            var inventory = new Inventory
            {

            };
            var data = new Account
            {
                UserId = user.Id,
                Active = true,
                Class = ClassNames.Shipgirl,
                Credit = 0,
                CreditSpecial = 0,
                CustomRoleId = null,
                DailyCredit = DateTime.UtcNow,
                GameKillAmount = 0,
                MvpCounter = 0,
                RepCooldown = DateTime.UtcNow,
                Rep = 0,
                Exp = 0,
                VoiceExpTime = DateTime.UtcNow,
                TotalExp = 0,
                MvpIgnore = false,
                MvpImmunity = false,
                Level = 1,
                Sessions = 0,
                TimeInVoice = TimeSpan.Zero,
                VoiceTime = DateTime.UtcNow,
                Inventory = new List<Inventory> { inventory }
            };
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user.Id);
        }

        public static async Task<ModLog> CreateCaseId(this DbService context, IUser user, DateTime time, ModAction action)
        {
            var data = new ModLog
            {
                UserId = user.Id,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ModLogs.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<ClubInfo> CreateClub(this DbService context, IUser user, string name, DateTime time)
        {
            var check = await context.ClubInfos.FindAsync(user.Id);
            if (check != null) return null;
            var data = new ClubInfo
            {
                Leader = user.Id,
                Name = name,
                CreationDate = time
            };
            await context.ClubInfos.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.CreationDate == time);
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, DateTime time)
        {
            var data = new Suggestion
            {
                Date = time,
                UserId = user.Id,
                Status = true,
            };
            await context.Suggestions.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Suggestions.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<Report> CreateReport(this DbService context, IUser user, DateTime time)
        {
            var data = new Report
            {
                UserId = user.Id,
                Status = true,
                Date = time
            };
            await context.Reports.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Reports.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<GuildConfig> GetOrCreateGuildConfig(this DbService context, SocketGuild guild)
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
                StackLvlRoles = false,
                AntiSpam = null,
                ExpMultiplier = 1,
                MuteRole = null,
                WelcomeLimit = 10
            };
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild.Id);
        }
    }
}
