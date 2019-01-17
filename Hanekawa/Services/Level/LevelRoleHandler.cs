using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Level
{
    public class LevelRoleHandler : IHanaService
    {
        private readonly DiscordSocketClient _client;

        public LevelRoleHandler(DiscordSocketClient client) => _client = client;

        public async Task NewLevelManagerAsync(DbService db, Account userdata, IGuildUser user)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild as SocketGuild);

            if (role == null)
            {
                await RoleCheckAsync(db, user, cfg, userdata);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.TryAddRoleAsync(role);
        }

        public async Task GiveRolesBackAsync(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                if (userdata == null || userdata.Level < 2) return;
                var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild);
                if (cfg.StackLvlRoles)
                {
                    var roleCollection = await GetRoleCollectionAsync(db, user, userdata);
                    await user.TryAddRolesAsync(roleCollection);
                    return;
                }

                var singleRole = await GetRoleSingleAsync(db, user, userdata);
                await user.TryAddRolesAsync(singleRole);
            }
        }

        private async Task RoleCheckAsync(DbService db, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            List<IRole> roles;
            if (cfg.StackLvlRoles) roles = await GetRoleCollectionAsync(db, user, userdata);
            else roles = await GetRoleSingleAsync(db, user, userdata);
            var missingRoles = new List<IRole>();
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in roles)
                if (!user.RoleIds.Contains(x.Id) && (currentUser as SocketGuildUser).HierarchyCheck(x))
                    missingRoles.Add(x);

            if (missingRoles.Count == 0) return;
            await user.TryAddRolesAsync(missingRoles);
        }

        private async Task<List<IRole>> GetRoleSingleAsync(DbService db, IGuildUser user, Account userdata)
        {
            var roles = new List<IRole>();
            ulong role = 0;
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
            {
                if (userdata.Level >= x.Level && x.Stackable)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if ((currentUser as SocketGuildUser).HierarchyCheck(getRole)) roles.Add(getRole);
                }

                if (userdata.Level >= x.Level) role = x.Role;
            }

            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if ((currentUser as SocketGuildUser).HierarchyCheck(getSingleRole)) roles.Add(getSingleRole);
            return roles;
        }

        private async Task<List<IRole>> GetRoleCollectionAsync(DbService db, IGuildUser user, Account userdata)
        {
            var roles = new List<IRole>();
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
                if (userdata.Level >= x.Level)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if ((currentUser as SocketGuildUser).HierarchyCheck(getRole)) roles.Add(getRole);
                }

            return roles;
        }

        private IRole GetLevelUpRole(uint level, IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleId = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleId == null ? null : _client.GetGuild(user.GuildId).GetRole(roleId.Role);
        }

        private static async Task RemoveLevelRolesAsync(IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            foreach (var x in rolesRewards)
            {
                if (x.Stackable) continue;
                if (user.RoleIds.Contains(x.Role)) await user.TryRemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }
    }
}