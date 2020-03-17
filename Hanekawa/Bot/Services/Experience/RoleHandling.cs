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
                using var db = new DbService();

                var userData = await db.GetOrCreateUserData(user);
                if (userData == null || userData.Level < 2) return;

                var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                var roles = await GetRolesAsync(user, userData, db, cfg.StackLvlRoles);
                await user.TryAddRolesAsync(roles);
            });
            return Task.CompletedTask;
        }

        private async Task NewLevelManagerAsync(Account userData, SocketGuildUser user, DbService db)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync();
            if (roles == null || roles.Count == 0) return;

            var lvRole = roles.FirstOrDefault(x => x.Level == userData.Level);
            var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);

            if (lvRole == null)
            {
                await RoleCheckAsync(user, cfg, userData, db);
                return;
            }
            var role = user.Guild.GetRole(lvRole.Role);
            if (role == null)
            {
                db.LevelRewards.Remove(lvRole);
                await db.SaveChangesAsync();
                return;
            }
            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.TryAddRoleAsync(role);
        }

        private async Task RoleCheckAsync(SocketGuildUser user, LevelConfig cfg, Account userData, DbService db)
        {
            var roles = await GetRolesAsync(user, userData, db, cfg.StackLvlRoles);

            var missingRoles = new List<SocketRole>();
            var currentUser = user.Guild.CurrentUser;
            for (var i = 0; i < roles.Count; i++)
            {
                var x = roles[i];
                if (!user.Roles.Contains(x) && currentUser.HierarchyCheck(x))
                    missingRoles.Add(x);
            }

            if (missingRoles.Count == 0) return;
            await user.TryAddRolesAsync(missingRoles);
        }

        private async Task<List<SocketRole>> GetRolesAsync(SocketGuildUser user, Account userData, DbService db,
            bool stack)
        {
            var roles = new List<SocketRole>();
            ulong role = 0;
            var currentUser = user.Guild.CurrentUser;
            var roleList = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync();

            for (var i = 0; i < roleList.Count; i++)
            {
                var x = roleList[i];
                var getRole = user.Guild.GetRole(x.Role);
                if(getRole == null) continue;
                if (stack)
                {
                    if (userData.Level < x.Level) continue;
                    if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                }
                else
                {
                    if (userData.Level >= x.Level && x.Stackable)
                    {
                        if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                    }

                    if (userData.Level >= x.Level) role = x.Role;
                }
            }

            if (stack) return roles;
            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if (currentUser.HierarchyCheck(getSingleRole)) roles.Add(getSingleRole);
            return roles;
        }

        private async Task RemoveLevelRolesAsync(SocketGuildUser user, List<LevelReward> rolesRewards)
        {
            for (var i = 0; i < rolesRewards.Count; i++)
            {
                var x = rolesRewards[i];
                if (x.Stackable) continue;
                if (user.Roles.Contains(user.Guild.GetRole(x.Role)))
                    await user.TryRemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }
    }
}