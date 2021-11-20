using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Service.Experience
{
    public partial class ExpService
    {
        public int ExpToNextLevel(Account userData) => ExpToNextLevel(userData.Level);
        public int ExpToNextLevel(AccountGlobal userData) => 50 * userData.Level * userData.Level + 300; 
        public int ExpToNextLevel(int level) => 3 * level * level + 150;

        public async Task<int> AddExpAsync(IMember user, Account userData, int exp, int credit, DbService db, ExpSource source)
        {
            if (user.BoostedAt.HasValue)
            {
                var cfg = await db.GetOrCreateEntityAsync<LevelConfig>(user.GuildId);
                exp = Convert.ToInt32(exp * cfg.BoostExpMultiplier);
            }
            exp = Convert.ToInt32(exp * await GetMultiplier(source, user.GuildId, db));
            
            if (userData.Exp + exp >= ExpToNextLevel(userData.Level))
            {
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData.Level);
                userData.Level += 1;
                await LevelUpCheckAsync(user, userData, db, userData.Decay);
                // TODO: Server Achieve await _achievement.ServerLevel(user, userData, db);
                Logger.LogInformation("(Exp Service | Server) {GuildId} - {UserId} Leveled up {Level} and gained {Exp} exp {Credit} credit", 
                    userData.GuildId, userData.UserId, userData.Level, exp, credit);
            }
            else if (userData.Exp + exp < 0)
            {
                userData.Level -= 1;
                userData.Exp = userData.Exp + ExpToNextLevel(userData.Level - 1) + exp;
            }
            else
            {
                userData.Exp += exp;
                Logger.LogInformation("(Exp Service | Server) {GuildId} - {UserId} gained {Exp} exp {Credit} credit",
                    userData.GuildId, userData.UserId, exp, credit);
            }
            
            if (userData.Decay != 0) userData.Decay = 0;
            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
            return exp;
        }

        public async Task<int> AddExpAsync(AccountGlobal userData, int exp, int credit, DbService db)
        {
            if (userData.Exp + exp >= ExpToNextLevel(userData))
            {
                userData.Level += 1;
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData);
                Logger.LogInformation("(Exp Service | Global) {UserId} Leveled up {Level} and gained {Exp} exp {Credit} credit", userData.UserId, userData.Level, exp, credit);
            }
            else
            {
                userData.Exp += exp;
                Logger.LogInformation("(Exp Service | Global) {UserId} gained {Exp} exp {Credit} credit", userData.UserId, exp, credit);
            }

            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
            
            return exp;
        }

        private async Task<double> GetMultiplier(ExpSource source, Snowflake guildId, DbService db)
        {
            if(_cache.TryGetMultiplier(source, guildId, out var value)) return value;
            var cfg = await db.GetOrCreateEntityAsync<LevelConfig>(guildId);
            _cache.AdjustExpMultiplier(ExpSource.Text, guildId, cfg.TextExpMultiplier);
            _cache.AdjustExpMultiplier(ExpSource.Voice, guildId, cfg.VoiceExpMultiplier);
            _cache.AdjustExpMultiplier(ExpSource.Other, guildId, cfg.TextExpMultiplier);
            
            return source == ExpSource.Voice 
                ? cfg.VoiceExpMultiplier 
                : cfg.TextExpMultiplier;
        }
    }
}