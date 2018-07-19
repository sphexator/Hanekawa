﻿using Discord;
using Discord.WebSocket;
using Jibril.Data.Constants;
using Jibril.Data.Variables;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Jibril.Extensions
{
    public static class DbExtension
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, SocketGuildUser user)
        {
            var userdata = await context.Accounts.FindAsync(user.Id);
            if (userdata != null) return userdata;
            var data = new Account
            {
                UserId = user.Id,
                GuildId = user.Guild.Id,
                Active = true,
                Class = ClassNames.Shipgirl,
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
                TimeInVoice = TimeSpan.Zero,
                VoiceTime = DateTime.UtcNow
            };
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user.Id);
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
                Rep = 0
            };
            await context.AccountGlobals.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AccountGlobals.FindAsync(user.Id);
        }

        public static async Task<ModLog> CreateCaseId(this DbService context, IUser user, SocketGuild guild, DateTime time, ModAction action)
        {
            var counter = await context.ModLogs.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0) nr = 1;
            else nr = (uint)counter + 1;
            var data = new ModLog
            {
                Id = nr,
                GuildId = guild.Id,
                UserId = user.Id,
                Date = time,
                Action = action.ToString()
            };
            await context.ModLogs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ModLogs.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<ClubInfo> CreateClub(this DbService context, IUser user, SocketGuild guild, string name, DateTime time)
        {
            var check = await context.ClubInfos.FindAsync(user.Id);
            if (check != null) return null;
            var counter = await context.ClubInfos.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0) nr = 1;
            else nr = (uint)counter + 1;
            var data = new ClubInfo
            {
                Id = nr,
                GuildId = guild.Id,
                Leader = user.Id,
                Name = name,
                CreationDate = time
            };
            await context.ClubInfos.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.CreationDate == time);
        }

        public static async Task<Suggestion> CreateSuggestion(this DbService context, IUser user, SocketGuild guild, DateTime time)
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

        public static async Task<Report> CreateReport(this DbService context, IUser user, SocketGuild guild, DateTime time)
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
                StackLvlRoles = true,
                ExpMultiplier = 1,
                MuteRole = null,
                WelcomeLimit = 5,
                Prefix = "h.",
                BoardChannel = null
            };
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild.Id);
        }
    }
}
