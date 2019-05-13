using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private Task GiveRolesBackAsync(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    if (userdata == null || userdata.Level < 2) return;
                    var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                    if (cfg.StackLvlRoles)
                    {
                        var roleCollection = await GetRoleCollectionAsync(user, userdata, db);
                        await user.TryAddRolesAsync(roleCollection);
                        return;
                    }

                    var singleRole = await GetRoleSingleAsync(user, userdata, db);
                    await user.TryAddRolesAsync(singleRole);
                }
            });
            return Task.CompletedTask;
        }
        
        private async Task NewLevelManagerAsync(Account userdata, SocketGuildUser user, DbService db)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);

            if (role == null)
            {
                await RoleCheckAsync(user, cfg, userdata, db);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.TryAddRoleAsync(role);
        }

        private async Task RoleCheckAsync(SocketGuildUser user, LevelConfig cfg, Account userdata, DbService db)
        {
            List<SocketRole> roles;
            if (cfg.StackLvlRoles) roles = await GetRoleCollectionAsync(user, userdata, db);
            else roles = await GetRoleSingleAsync(user, userdata, db);
            
            var missingRoles = new List<SocketRole>();
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in roles)
                if (!user.Roles.Contains(x) && currentUser.HierarchyCheck(x))
                    missingRoles.Add(x);

            if (missingRoles.Count == 0) return;
            await user.TryAddRolesAsync(missingRoles);
        }

        private async Task<List<SocketRole>> GetRoleSingleAsync(SocketGuildUser user, Account userdata, DbService db)
        {
            var roles = new List<SocketRole>();
            ulong role = 0;
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync())
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

        private async Task<List<SocketRole>> GetRoleCollectionAsync(SocketGuildUser user, Account userdata, DbService db)
        {
            var roles = new List<SocketRole>();
            var currentUser = user.Guild.CurrentUser;
            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync())
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