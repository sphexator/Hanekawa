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
                UserId = user.Id,
                CustomRole = 0,
                DamageBoost = 0,
                Gift = 0,
                RepairKit = 0,
                Shield = 0
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
                Level = 0,
                Inventory = new List<Inventory> { inventory }
            };
            await context.Accounts.AddAsync(data);
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
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.CreationDate == time);
        }

        public static async Task<GuildConfig> GetOrCreateGuildConfig(this DbService context, SocketGuild guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild.Id);
            if (response != null) return response;
            var data = new GuildConfig
            {
                GuildId = guild.Id,
                Welcome = true
            };
            await context.GuildConfigs.AddAsync(data);
            return await context.GuildConfigs.FindAsync(guild.Id);
        }
    }
}
