using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Experience
{
    public partial class ExpService
    {
        private Task GiveRolesBackAsync(MemberJoinedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.Member;
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();

                var userData = await db.GetOrCreateUserData(user);
                if (userData == null || userData.Level < 2) return;

                var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);
                var levelRewards = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id.RawValue).ToListAsync();
                var roles = GetRolesAsync(user, userData, db, cfg.StackLvlRoles, levelRewards, 0);
                await user.TryAddRolesAsync(roles);
            });
            return Task.CompletedTask;
        }

        private Task RemoveRole(RoleDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                    var role = await db.LevelRewards.FirstOrDefaultAsync(x =>
                        x.GuildId == e.Role.Guild.Id.RawValue && x.Role == e.Role.Id.RawValue);
                    if (role == null) return;
                    db.LevelRewards.Remove(role);
                    await db.SaveChangesAsync();
                    _log.Log(NLog.LogLevel.Info, $"Removed Level Reward from guild {e.Role.Guild.Id.RawValue} for level {role.Level} - role id: {e.Role.Id.RawValue}");
                }
                catch (Exception exception)
                {
                    _log.Log(NLog.LogLevel.Error, exception, exception.Message);
                }
            });
            return Task.CompletedTask;
        }

        public async Task NewLevelManagerAsync(Account userData, CachedMember user, DbService db, int levelDecay = 0)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id.RawValue).ToListAsync();
            if (roles == null || roles.Count == 0) return;

            var lvRole = roles.FirstOrDefault(x => (x.Level - levelDecay) == userData.Level);
            var cfg = await db.GetOrCreateLevelConfigAsync(user.Guild);

            if (lvRole == null)
            {
                await RoleCheckAsync(user, cfg, userData, roles, db, levelDecay);
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
            await RoleCheckAsync(user, cfg, userData, roles, db, levelDecay);
        }

        private static async Task RoleCheckAsync(CachedMember user, LevelConfig cfg, Account userData, List<LevelReward> levelRoles, DbService db, int levelDecay)
        {
            var roles = GetRolesAsync(user, userData, db, cfg.StackLvlRoles, levelRoles, levelDecay);

            var missingRoles = new List<CachedRole>();
            var toRemove = new List<CachedRole>();
            var currentUser = user.Guild.CurrentMember;
            for (var i = 0; i < roles.Count; i++)
            {
                var x = roles[i];
                if (!user.Roles.Values.Contains(x) && currentUser.HierarchyCheck(x))
                    missingRoles.Add(x);
            }

            for (var i = 0; i < levelRoles.Count; i++)
            {
                var x = levelRoles[i];
                if(x.Level <= userData.Level - levelDecay) continue;
                if (user.Roles.ContainsKey(x.Role))
                {
                    if(x.NoDecay && userData.Level >= x.Level) continue;
                    var remove = user.Guild.GetRole(x.Role);
                    if(remove == null) continue;
                    toRemove.Add(remove);
                }
            }

            if (missingRoles.Count != 0) await user.TryAddRolesAsync(missingRoles);
            if (toRemove.Count != 0) await user.TryRemoveRolesAsync(toRemove);
        }

        private static List<CachedRole> GetRolesAsync(CachedMember user, Account userData, DbService db,
            bool stack, List<LevelReward> roleList, int levelDecay)
        {
            var roles = new List<CachedRole>();
            ulong role = 0;
            var currentUser = user.Guild.CurrentMember;

            foreach (var x in roleList)
            {
                var getRole = user.Guild.GetRole(x.Role);
                if (getRole == null) continue;
                if (stack)
                {
                    if ((userData.Level - levelDecay) < x.Level) continue;
                    if (currentUser.HierarchyCheck(getRole)) roles.Add(getRole);
                }
                else
                {
                    if ((userData.Level - levelDecay) >= x.Level && x.Stackable && currentUser.HierarchyCheck(getRole))
                    {
                        roles.Add(getRole);
                    }

                    if ((userData.Level - levelDecay) >= x.Level) role = x.Role;
                }
            }

            if (stack) return roles;
            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if (currentUser.HierarchyCheck(getSingleRole)) roles.Add(getSingleRole);
            return roles;
        }

        private static async Task RemoveLevelRolesAsync(CachedMember user, List<LevelReward> rolesRewards)
        {
            for (var i = 0; i < rolesRewards.Count; i++)
            {
                var x = rolesRewards[i];
                if (x.Stackable) continue;
                if (user.Roles.Values.Contains(user.Guild.GetRole(x.Role)))
                    await user.TryRemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }
    }
}