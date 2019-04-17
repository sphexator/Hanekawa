using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : INService
    {
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;
        private readonly ConcurrentDictionary<ulong, int> _expMultiplier = new ConcurrentDictionary<ulong, int>();
        
        public ExpService(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }
        
        public int ExpToNextLevel(Account userdata) => 3 * userdata.Level * userdata.Level + 150;
        public int ExpToNextLevel(AccountGlobal userdata) => 50 * userdata.Level * userdata.Level + 300;

        public async Task AddExp(SocketGuildUser user, Account userdata, int exp, int credit)
        {
            if (userdata.Exp + exp >= ExpToNextLevel(userdata))
            {
                var roles = await GetRoles(user.Guild.Id);
                if (roles != null) await AddRoles(user, roles);
                userdata.Level += 1;
                userdata.Exp = (userdata.Exp + exp - ExpToNextLevel(userdata));
            }
            else userdata.Exp += exp;
            userdata.TotalExp += exp;
            userdata.Credit += credit;
            await _db.SaveChangesAsync();
        }
        
        public async Task AddExp(SocketGuildUser user, AccountGlobal userdata, int exp, int credit)
        {
            
        }

        public void AdjustMultiplier(ulong guildId, int multiplier) 
            => _expMultiplier.AddOrUpdate(guildId, multiplier, (key, value) => multiplier);

        private async Task<List<SocketRole>> GetRoles(ulong guildId)
        {
            var roles = await _db.LevelRewards.Where(x => x.GuildId == guildId).ToListAsync();
            if (roles.Count == 0) return null;
            var result = new List<SocketRole>();
            var guild = _client.GetGuild(guildId);
            foreach (var x in roles)
            {
                var role = guild.GetRole(x.Role);
                if (role == null)
                {
                    _db.LevelRewards.Remove(x);
                    continue;
                }
                result.Add(role);
            }

            return result.Count == 0 ? null : result;
        }

        private async Task AddRoles(SocketGuildUser user, List<SocketRole> roles)
        {
            var cfg = await _db.GetOrCreateLevelConfigAsync(user.Guild.Id);
            if (cfg.StackLvlRoles)
            {
                
            }
            else
            {
                
            }
        }

        private async Task RemoveRole()
        {
            
        }
    }
}