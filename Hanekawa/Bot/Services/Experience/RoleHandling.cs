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
            var role = GetLevelUpRole(userData.Level, user, roles);
            var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);

            if (role == null)
            {
                await RoleCheckAsync(user, cfg, userData, db);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.TryAddRoleAsync(role);
        }

        private async Task RoleCheckAsync(SocketGuildUser user, LevelConfig cfg, Account userData, DbService db)
        {
            var roles = await GetRolesAsync(user, userData, db, cfg.StackLvlRoles).ConfigureAwait(false);

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

        private async ValueTask<List<SocketRole>> GetRolesAsync(SocketGuildUser user, Account userData, DbService db,
            bool stack)
        {
            var roles = new List<SocketRole>();
            ulong role = 0;
            var currentUser = user.Guild.CurrentUser;
            var roleList = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id).ToListAsync();

            for (var i = 0; i < roleList.Count; i++)
            {
                var x = roleList[i];
                if (stack)
                {
                    if (userData.Level < x.Level) continue;
                    var getRole = user.Guild.GetRole(x.Role);
                    if (getRole == null) continue;
                    if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                }
                else
                {
                    if (userData.Level >= x.Level && x.Stackable)
                    {
                        var getRole = user.Guild.GetRole(x.Role);
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

        private SocketRole GetLevelUpRole(int level, SocketGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleId = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleId == null ? null : _client.GetGuild(user.Guild.Id).GetRole(roleId.Role);
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