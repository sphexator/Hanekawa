using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Microsoft.Extensions.Logging;

#nullable enable

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private readonly ConcurrentDictionary<ulong, double> _textExpMultiplier =
            new ConcurrentDictionary<ulong, double>();

        private readonly ConcurrentDictionary<ulong, double> _voiceExpMultiplier =
            new ConcurrentDictionary<ulong, double>();

        public int ExpToNextLevel(Account userdata) => 3 * userdata.Level * userdata.Level + 150;

        private static int ExpToNextLevel(AccountGlobal userdata) => 50 * userdata.Level * userdata.Level + 300;

        public int ExpToNextLevel(int level) => 3 * level * level + 150;

        public async Task<int> AddExpAsync(CachedMember user, Account userData, int exp, int credit, DbService db)
        {
            if (user.IsBoosting)
            {
                var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                exp = Convert.ToInt32(exp * cfg.BoostExpMultiplier);
            }

            if (userData.Exp + exp >= ExpToNextLevel(userData))
            {
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData);
                userData.Level += 1;
                await NewLevelManagerAsync(userData, user, db);
                _log.LogAction(LogLevel.Information,
                    $"(Exp Service | Server) {userData.UserId} Leveled up {userData.Level} and gained {exp} exp {credit} credit");
            }
            else if (userData.Exp + exp < 0)
            {
                userData.Level -= 1;
                userData.Exp = userData.Exp + ExpToNextLevel(userData.Level - 1) + exp;
                _log.LogAction(LogLevel.Information,
                    $"(Exp Service | Server) {userData.UserId} de-leveled to {userData.Level} and gained {exp} exp {credit} credit");
            }
            else
            {
                userData.Exp += exp;
                _log.LogAction(LogLevel.Information,
                    $"(Exp Service | Server) {userData.UserId} gained {exp} exp {credit} credit");
            }

            if(userData.Decay != 0) userData.Decay = 0;
            userData.TotalExp += exp;
            userData.Credit += credit;
            if (!userData.Active) userData.Active = true;
            await db.SaveChangesAsync();
            return exp;
        }

        private async Task AddExpAsync(AccountGlobal userData, int exp, int credit, DbService db)
        {
            if (userData.Exp + exp >= ExpToNextLevel(userData))
            {
                userData.Level += 1;
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData);
                _log.LogAction(LogLevel.Information,
                    $"(Exp Service | Global) {userData.UserId} Leveled up {userData.Level} and gained {exp} exp {credit} credit");
            }
            else
            {
                userData.Exp += exp;
                _log.LogAction(LogLevel.Information,
                    $"(Exp Service | Global) {userData.UserId} gained {exp} exp {credit} credit");
            }

            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
        }

        public void AdjustTextMultiplier(ulong guildId, double multiplier) 
            => _textExpMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public void AdjustVoiceMultiplier(ulong guildId, double multiplier) 
            => _voiceExpMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public double GetMultiplier(ulong guildId)
        {
            return _textExpMultiplier.GetOrAdd(guildId, 1);
        }
    }
}