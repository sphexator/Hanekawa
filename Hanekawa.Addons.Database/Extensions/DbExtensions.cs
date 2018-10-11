using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.BoardConfig;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Tables.Stores;

namespace Hanekawa.Addons.Database.Extensions
{
    public static class DbExtensions
    {
        public static async Task<HungerGameConfig> GetOrCreateHungerGameConfig(this DbService context,
            SocketGuild guild)
        {
            var config = await context.HungerGameConfigs.FindAsync(guild.Id);
            if (config != null)
            {
                return config;
            }

            var data = new HungerGameConfig
            {
                GuildId = guild.Id,
                Live = false,
                MessageId = 0,
                Round = 0,
                SignupStage = false,
                SignupTime = DateTime.UtcNow - TimeSpan.FromDays(2),
                WinSpecialCredit = 500,
                WinnerRoleId = null,
                WinExp = 1000,
                WinCredit = 1000
            };
            await context.HungerGameConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.HungerGameConfigs.FindAsync(guild.Id);
        }

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

        public static async Task<Account> GetOrCreateUserData(this DbService context, SocketGuildUser user)
        {
            var userdata = await context.Accounts.FindAsync(user.Guild.Id, user.Id);
            if (userdata != null)
            {
                return userdata;
            }

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
            var userdata = await context.Accounts.FindAsync(guild.Id, user.Id);
            if (userdata != null)
            {
                return userdata;
            }

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
            var userdata = await context.Accounts.FindAsync(guild, user);
            if (userdata != null)
            {
                return userdata;
            }

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
            if (userdata != null)
            {
                return userdata;
            }

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

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, ulong userId)
        {
            var userdata = await context.AccountGlobals.FindAsync(userId);
            if (userdata != null)
            {
                return userdata;
            }

            var data = new AccountGlobal
            {
                UserId = userId,
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
            return await context.AccountGlobals.FindAsync(userId);
        }

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

        public static async Task<GuildConfig> GetOrCreateGuildConfig(this DbService context, IGuild guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild.Id);
            if (response != null)
            {
                return response;
            }

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
                SuggestionEmoteNo = "<:2no:403870492206825472>",
                ClubAdvertisementChannel = null,
                ClubChannelCategory = null,
                ClubChannelRequiredAmount = 4,
                ClubChannelRequiredLevel = 40,
                ClubEnableVoiceChannel = false
            };
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild.Id);
        }
    }
}
