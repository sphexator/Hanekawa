using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Experience
{
    // TODO Role service for exp service
    public partial class ExpService
    {
        private readonly ConcurrentDictionary<ulong, double> _expMultiplier = new ConcurrentDictionary<ulong, double>();

        public int ExpToNextLevel(Account userdata) => 3 * userdata.Level * userdata.Level + 150;
        public int ExpToNextLevel(AccountGlobal userdata) => 50 * userdata.Level * userdata.Level + 300;
        public int ExpToNextLevel(int level) => 3 * level * level + 150;

        public async Task AddExpAsync(SocketGuildUser user, Account userdata, int exp, int credit, DbService db)
        {
            if (userdata.Exp + exp >= ExpToNextLevel(userdata))
            {
                var roles = await GetRoles(user.Guild.Id, db);
                if (roles != null) await AddRoles(user, db, roles);
                userdata.Level += 1;
                userdata.Exp = (userdata.Exp + exp - ExpToNextLevel(userdata));
            }
            else if (userdata.Exp + exp <= 0)
            {
                userdata.Level -= 1;
                userdata.Exp = userdata.Exp + ExpToNextLevel(userdata.Level - 1) + exp;
            }
            else userdata.Exp += exp;
            userdata.TotalExp += exp;
            userdata.Credit += credit;
            await db.SaveChangesAsync();
        }
        
        public async Task AddExpAsync(AccountGlobal userdata, int exp, int credit, DbService db)
        {
            if (userdata.Exp + exp >= ExpToNextLevel(userdata))
            {
                userdata.Level += 1;
                userdata.Exp = (userdata.Exp + exp - ExpToNextLevel(userdata));
            }
            else userdata.Exp += exp;
            userdata.TotalExp += exp;
            userdata.Credit += credit;
            await db.SaveChangesAsync();
        }

        public void AdjustMultiplier(ulong guildId, double multiplier) 
            => _expMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        public double GetMultiplier(ulong guildId) => _expMultiplier.GetOrAdd(guildId, 1);

        private async Task<List<SocketRole>> GetRoles(ulong guildId, DbService db)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == guildId).ToListAsync();
            if (roles.Count == 0) return null;
            var result = new List<SocketRole>();
            var guild = _client.GetGuild(guildId);
            foreach (var x in roles)
            {
                var role = guild.GetRole(x.Role);
                if (role == null)
                {
                    db.LevelRewards.Remove(x);
                    continue;
                }
                result.Add(role);
            }

            return result.Count == 0 ? null : result;
        }

        private async Task AddRoles(SocketGuildUser user, DbService db, List<SocketRole> roles)
        {
            var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild.Id);
            if (cfg.StackLvlRoles)
            {
                await user.TryAddRolesAsync(roles);
            }
            else
            {
                // TODO: Look into this later
            }
        }

        private async Task RemoveRole()
        {
            
        }
    }
}