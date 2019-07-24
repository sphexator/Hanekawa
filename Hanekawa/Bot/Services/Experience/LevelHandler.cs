using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Account;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private readonly ConcurrentDictionary<ulong, double> _textExpMultiplier =
            new ConcurrentDictionary<ulong, double>();

        private readonly ConcurrentDictionary<ulong, double> _voiceExpMultiplier =
            new ConcurrentDictionary<ulong, double>();

        public int ExpToNextLevel(Account userdata) => 3 * userdata.Level * userdata.Level + 150;
        private int ExpToNextLevel(AccountGlobal userdata) => 50 * userdata.Level * userdata.Level + 300;
        public int ExpToNextLevel(int level) => 3 * level * level + 150;

        public async Task AddExpAsync(SocketGuildUser user, Account userData, int exp, int credit, DbService db)
        {
            if (userData.Exp + exp >= ExpToNextLevel(userData))
            {
                await NewLevelManagerAsync(userData, user, db);
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData);
                userData.Level += 1;
            }
            else if (userData.Exp + exp < 0)
            {
                userData.Level -= 1;
                userData.Exp = userData.Exp + ExpToNextLevel(userData.Level - 1) + exp;
            }
            else
            {
                userData.Exp += exp;
            }

            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
        }

        private async Task AddExpAsync(AccountGlobal userData, int exp, int credit, DbService db)
        {
            if (userData.Exp + exp >= ExpToNextLevel(userData))
            {
                userData.Level += 1;
                userData.Exp = userData.Exp + exp - ExpToNextLevel(userData);
            }
            else
            {
                userData.Exp += exp;
            }

            userData.TotalExp += exp;
            userData.Credit += credit;
            await db.SaveChangesAsync();
        }

        public void AdjustTextMultiplier(ulong guildId, double multiplier)
            => _textExpMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public void AdjustVoiceMultiplier(ulong guildId, double multiplier)
            => _voiceExpMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public double GetMultiplier(ulong guildId) => _textExpMultiplier.GetOrAdd(guildId, 1);
    }
}