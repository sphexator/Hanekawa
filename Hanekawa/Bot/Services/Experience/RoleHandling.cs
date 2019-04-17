using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService : IRequired
    {
        public ExpService() => _client.UserJoined += GiveRolesBackAsync;

        private async Task GiveRolesBackAsync(SocketGuildUser user)
        {
            var userdata = await _db.GetOrCreateUserData(user);
            if (userdata == null || userdata.Level < 2) return;
            var cfg = await _db.GetOrCreateLevelConfigAsync(user.Guild);
            if (cfg.StackLvlRoles)
            {
                var roleCollection = await GetRoleCollectionAsync(user, userdata);
                await user.TryAddRolesAsync(roleCollection);
                return;
            }

            var singleRole = await GetRoleSingleAsync(user, userdata);
            await user.TryAddRolesAsync(singleRole);
        }
        
        private async Task NewLevelManagerAsync(Account userdata, SocketGuildUser user)
        {
            var roles = await _db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            var cfg = await _db.GetOrCreateLevelConfigAsync(user.Guild as SocketGuild);

            if (role == null)
            {
                await RoleCheckAsync(user, cfg, userdata);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.TryAddRoleAsync(role);
        }

        private async Task RoleCheckAsync(SocketGuildUser user, LevelConfig cfg, Account userdata)
        {
            List<SocketRole> roles;
            if (cfg.StackLvlRoles) roles = await GetRoleCollectionAsync(user, userdata);
            else roles = await GetRoleSingleAsync(user, userdata);
            
            var missingRoles = new List<SocketRole>();
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in roles)
                if (!user.Roles.Contains(x) && (currentUser as SocketGuildUser).HierarchyCheck(x))
                    missingRoles.Add(x);

            if (missingRoles.Count == 0) return;
            await user.TryAddRolesAsync(missingRoles);
        }

        private async Task<List<SocketRole>> GetRoleSingleAsync(SocketGuildUser user, Account userdata)
        {
            var roles = new List<SocketRole>();
            ulong role = 0;
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in await _db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync())
            {
                if (userdata.Level >= x.Level && x.Stackable)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                }

                if (userdata.Level >= x.Level) role = x.Role;
            }

            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if (currentUser.HierarchyCheck(getSingleRole)) roles.Add(getSingleRole);
            return roles;
        }

        private async Task<List<SocketRole>> GetRoleCollectionAsync(SocketGuildUser user, Account userdata)
        {
            var roles = new List<SocketRole>();
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in await _db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync())
                if (userdata.Level >= x.Level)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if (getRole == null) continue;
                    if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                }

            return roles;
        }

        private SocketRole GetLevelUpRole(int level, SocketGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleId = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleId == null ? null : _client.GetGuild(user.Guild.Id).GetRole(roleId.Role);
        }

        private async Task RemoveLevelRolesAsync(SocketGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            foreach (var x in rolesRewards)
            {
                if (x.Stackable) continue;
                if (user.Roles.Contains(user.Guild.GetRole(x.Role))) await user.TryRemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }
    }
}