﻿using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Addons.Database.Tables.BoardConfig;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Addons.Database.Extensions
{
    public static class DbExtensions
    {
        public static async Task<EventPayout> GetOrCreateEventParticipant(this DbService context, SocketGuildUser user)
        {
            var userdata = await context.EventPayouts.FindAsync(user.Guild.Id, user.Id);
            if (userdata != null)
            {
                return userdata;
            }

            var data = new EventPayout
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Amount = 0
            };
            await context.EventPayouts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.EventPayouts.FindAsync(user.Guild.Id, user.Id);
        }

        public static async Task<Account> GetOrCreateUserData(this DbService context, SocketGuildUser user) =>
            await GetOrCreateServerUser(context, user.Guild.Id, user.Id);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user) =>
            await GetOrCreateServerUser(context, guild.Id, user.Id);

        public static async Task<Account> GetOrCreateUserData(this DbService context, ulong guild, ulong user) =>
            await GetOrCreateServerUser(context, guild, user);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuildUser user) =>
            await GetOrCreateServerUser(context, user.GuildId, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, ulong userId) =>
            await GetOrCreateGlobalUser(context, userId);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IGuildUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, SocketGuildUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, SocketUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateConfigAsync(context, guild.Id);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, SocketGuildUser user) =>
            await GetOrCreateConfigAsync(context, user.Guild.Id);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, SocketGuild guild) =>
            await GetOrCreateConfigAsync(context, guild.Id);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, ulong guild) =>
            await GetOrCreateConfigAsync(context, guild);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, ulong guild) =>
            GetOrCreateConfig(context, guild);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, SocketGuildUser guild) =>
            GetOrCreateConfig(context, guild.Guild.Id);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, IGuild guild) =>
            GetOrCreateConfig(context, guild.Id);

        public static async Task PurchaseServerItem(this DbService context, IGuildUser user, Item shop, int amount = 1)
        {
            var check = await context.Inventories.FindAsync(user.GuildId, user.Id, shop.ItemId);
            if (check != null)
            {
                check.Amount = check.Amount + amount;
                await context.SaveChangesAsync();
                return;
            }
            var data = new Inventory
            {
                GuildId = user.GuildId,
                UserId = user.Id,
                ItemId = shop.ItemId,
                Amount = amount
            };
            await context.Inventories.AddAsync(data);
            await context.SaveChangesAsync();
        }

        public static async Task PurchaseGlobalItem(this DbService context, IGuildUser user, Item shop, int amount = 1)
        {
            var check = await context.InventoryGlobals.FindAsync(user.Id, shop.ItemId);
            if (check != null)
            {
                check.Amount = check.Amount + amount;
                await context.SaveChangesAsync();
                return;
            }
            var data = new InventoryGlobal
            {
                UserId = user.Id,
                ItemId = shop.ItemId,
                Amount = amount
            };
            await context.InventoryGlobals.AddAsync(data);
            await context.SaveChangesAsync();
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
            var data = new ClubInfo
            {
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
            return await context.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.Leader == user.Id);
        }

        public static async Task<ClubInfo> GetClubAsync(this DbService context, int id, SocketGuild guild)
        {
            var check = await context.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == guild.Id);
            return check ?? null;
        }

        public static async Task<ClubInfo> IsClubLeader(this DbService context, ulong guild, ulong user)
        {
            try
            {
                var leader = await context.ClubInfos.FirstOrDefaultAsync(x =>
                    x.GuildId == guild && x.Leader == user);
                return leader;
            }
            catch { return null; }
        }

        public static async Task<Board> GetOrCreateBoard(this DbService context, IGuild guild, IUserMessage msg)
        {
            var check = await context.Boards.FindAsync(guild.Id, msg.Id);
            if (check != null)
            {
                return check;
            }

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
            if (counter == 0)
            {
                nr = 1;
            }
            else
            {
                nr = (uint)counter + 1;
            }

            var data = new Suggestion
            {
                Id = nr,
                GuildId = guild.Id,
                Date = time,
                UserId = user.Id,
                Status = true
            };
            await context.Suggestions.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Suggestions.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<QuestionAndAnswer> CreateQnA(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.QuestionAndAnswers.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0)
            {
                nr = 1;
            }
            else
            {
                nr = (uint)counter + 1;
            }

            var data = new QuestionAndAnswer
            {
                Id = nr,
                GuildId = guild.Id,
                Date = time,
                UserId = user.Id,
                Status = true
            };
            await context.QuestionAndAnswers.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.QuestionAndAnswers.FirstOrDefaultAsync(x => x.Date == time);
        }

        public static async Task<Report> CreateReport(this DbService context, IUser user, SocketGuild guild,
            DateTime time)
        {
            var counter = await context.Reports.CountAsync(x => x.GuildId == guild.Id);
            uint nr;
            if (counter == 0)
            {
                nr = 1;
            }
            else
            {
                nr = (uint)counter + 1;
            }

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

        public static async Task<AchievementTracker> GetAchievementProgress(this DbService context, IGuildUser user, int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, user.Id);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = user.Id
            };
            await context.AchievementTrackers.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AchievementTrackers.FindAsync(type, user.Id);
        }

        public static async Task<AchievementTracker> GetAchievementProgress(this DbService context, ulong userId, int type)
        {
            var check = await context.AchievementTrackers.FindAsync(type, userId);
            if (check != null) return check;
            var data = new AchievementTracker
            {
                Count = 0,
                Type = type,
                UserId = userId
            };
            await context.AchievementTrackers.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AchievementTrackers.FindAsync(type, userId);
        }

        private static async Task<Account> GetOrCreateServerUser(DbService context, ulong guild, ulong user)
        {
            var userdata = await context.Accounts.FindAsync(guild, user);
            if (userdata != null) return userdata;

            var data = new Account().DefaultAccount(guild, user);
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user, guild);
        }

        private static async Task<AccountGlobal> GetOrCreateGlobalUser(this DbService context, ulong userId)
        {
            var userdata = await context.AccountGlobals.FindAsync(userId);
            if (userdata != null) return userdata;
            
            var data = new AccountGlobal().DefaultAccountGlobal(userId);
            await context.AccountGlobals.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AccountGlobals.FindAsync(userId);
        }

        private static async Task<GuildConfig> GetOrCreateConfigAsync(DbService context, ulong guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild);
        }

        private static GuildConfig GetOrCreateConfig(DbService context, ulong guild)
        {
            var response = context.GuildConfigs.Find(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            context.GuildConfigs.Add(data);
            context.SaveChanges();
            return context.GuildConfigs.Find(guild);
        }
    }
}