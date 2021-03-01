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
                var levelRewards = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id.RawValue).ToArrayAsync();
                var roles = GetRolesAsync(user, userData, db, cfg.StackLvlRoles, levelRewards);
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
                    _log.LogAction(LogLevel.Information, $"Removed Level Reward from guild {e.Role.Guild.Id.RawValue} for level {role.Level} - role id: {e.Role.Id.RawValue}");
                }
                catch (Exception exception)
                {
                    _log.LogAction(LogLevel.Error, exception, exception.Message);
                }
            });
            return Task.CompletedTask;
        }

        public async Task NewLevelManagerAsync(Account userData, CachedMember user, DbService db, int levelDecay = 0)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.Guild.Id.RawValue).ToArrayAsync();
            if (roles == null || roles.Length == 0) return;

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

        private static async Task RoleCheckAsync(CachedMember user, LevelConfig cfg, Account userData, LevelReward[] levelRoles, DbService db, int levelDecay)
        {
            var roles = GetRolesAsync(user, userData, db, cfg.StackLvlRoles, levelRoles);

            var missingRoles = new CachedRole[levelRoles.Length];
            var toRemove = new CachedRole[levelRoles.Length];
            var currentUser = user.Guild.CurrentMember;
            for (var i = 0; i < roles.Length; i++)
            {
                var x = roles[i];
                if (!user.Roles.Values.Contains(x) && currentUser.HierarchyCheck(x))
                    missingRoles.SetValue(x, missingRoles.Length + 1);
            }

            for (var i = 0; i < levelRoles.Length; i++)
            {
                var x = levelRoles[i];
                if(x.Level <= userData.Level - levelDecay) continue;
                if (user.Roles.ContainsKey(x.Role))
                {
                    var remove = user.Guild.GetRole(x.Role);
                    if(remove == null) continue;
                    toRemove.SetValue(remove, toRemove.Length + 1);
                }
            }

            if (missingRoles.Length != 0) await user.TryAddRolesAsync(missingRoles);
            if (toRemove.Length != 0) await user.TryRemoveRolesAsync(toRemove);
        }

        private static CachedRole[] GetRolesAsync(CachedMember user, Account userData, DbService db,
            bool stack, LevelReward[] roleList)
        {
            var roles = new CachedRole[200];
            ulong role = 0;
            var currentUser = user.Guild.CurrentMember;

            for (var i = 0; i < roleList.Length; i++)
            {
                var x = roleList[i];
                var getRole = user.Guild.GetRole(x.Role);
                if (getRole == null) continue;
                if (stack)
                {
                    if (userData.Level < x.Level) continue;
                    if (currentUser.HierarchyCheck(getRole)) roles.SetValue(getRole, roles.Length + 1);
                }
                else
                {
                    if (userData.Level >= x.Level && x.Stackable && currentUser.HierarchyCheck(getRole))
                    {
                        roles.SetValue(getRole, roles.Length + 1);
                    }

                    if (userData.Level >= x.Level) role = x.Role;
                }
            }

            if (stack) return roles;
            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if (currentUser.HierarchyCheck(getSingleRole)) roles.SetValue(getSingleRole, roles.Length + 1);
            return roles;
        }

        private static async Task RemoveLevelRolesAsync(CachedMember user, LevelReward[] rolesRewards)
        {
            for (var i = 0; i < rolesRewards.Length; i++)
            {
                var x = rolesRewards[i];
                if (x.Stackable) continue;
                if (user.Roles.Values.Contains(user.Guild.GetRole(x.Role)))
                    await user.TryRemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }
    }
}