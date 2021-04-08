using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Entities;
using NLog;

namespace Hanekawa.Bot.Service
{
    public partial class Experience
    {
        public int ExpToNextLevel(Account userData) => ExpToNextLevel(userData.Level);
        public int ExpToNextLevel(AccountGlobal userData) => 50 * userData.Level * userData.Level + 300; 
        public int ExpToNextLevel(int level) => 3 * level * level + 150;

        public async Task<int> AddExpAsync(IMember user, Account userData, int exp, int credit, DbService db, ExpSource source)
        {
            if (user.BoostedAt.HasValue)
            {
                var cfg = await db.GetOrCreateLevelConfigAsync(user.GuildId.RawValue);
                exp = Convert.ToInt32(exp * cfg.BoostExpMultiplier);
            }

            exp = source == ExpSource.Voice
                ? Convert.ToInt32(exp * _cache.GetMultiplier(Database.Entities.ChannelType.Voice, user.GuildId))
                : Convert.ToInt32(exp * _cache.GetMultiplier(Database.Entities.ChannelType.Text, user.GuildId));
            
            if (userData.Exp + exp >= ExpToNextLevel(userData.Level))
            {
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData.Level);
                userData.Level += 1;
                _logger.Log(LogLevel.Info,
                    $"(Exp Service | Server) {userData.UserId} Leveled up {userData.Level} and gained {exp} exp {credit} credit");
            }
            else if (userData.Exp + exp < 0)
            {
                userData.Level -= 1;
                userData.Exp = userData.Exp + ExpToNextLevel(userData.Level - 1) + exp;
            }
            else
            {
                userData.Exp += exp;
                _logger.Log(LogLevel.Info,
                    $"(Exp Service | Server) {userData.UserId} gained {exp} exp {credit} credit");
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
                _logger.Log(LogLevel.Info,
                    $"(Exp Service | Global) {userData.UserId} Leveled up {userData.Level} and gained {exp} exp {credit} credit");
            }
            else
            {
                userData.Exp += exp;
                _logger.Log(LogLevel.Info,
                    $"(Exp Service | Global) {userData.UserId} gained {exp} exp {credit} credit");
            }

            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
            
            return exp;
        }
    }
}