using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Service
{
    public partial class Experience 
    {
        /// <summary>
        /// Check whether the user needs to have any roles added/removed
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userData"></param>
        /// <param name="db"></param>
        /// <param name="levelDecay"></param>
        /// <returns></returns>
        private async Task LevelUpCheckAsync(IMember user, Account userData, DbService db, int levelDecay = 0)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == userData.GuildId).ToListAsync();
            if (roles == null || roles.Count == 0) return;

            var role = roles.FirstOrDefault(x => x.Level == userData.Level - levelDecay);
            var cfg = await db.GetOrCreateLevelConfigAsync(user.GuildId);

            if (role == null)
            {
                await RoleCheckAsync(user, cfg, (userData.Level - levelDecay), roles);
                return;
            }
            
            if (!_bot.GetGuild(user.GuildId).Roles.TryGetValue(role.Role, out var toAdd))
            {
                db.LevelRewards.Remove(role);
                await db.SaveChangesAsync();
                return;
            }
            
            await RoleCheckAsync(user, cfg, (userData.Level - levelDecay), roles);
        }
        
        /// <summary>
        /// Check if the user has all the roles they're supposed to have, along with remove all unqualified roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cfg"></param>
        /// <param name="level"></param>
        /// <param name="lvlRoles"></param>
        /// <returns></returns>
        private async Task RoleCheckAsync(IMember user, LevelConfig cfg, int level, List<LevelReward> lvlRoles)
        {
            var guild = _bot.GetGuild(user.GuildId);
            var currentRoles = user.GetRoles().Keys.ToList();
            var currentUser = _bot.GetGuild(user.GuildId).GetCurrentUser();
            foreach (var x in GetRoles(user, guild, level, lvlRoles, cfg.StackLvlRoles)
                .Where(x => !currentRoles.Contains(x.Id) && guild.HierarchyCheck(currentUser, x)))
                currentRoles.Add(x.Id);
            
            foreach (var x in lvlRoles.Where(x => x.Level > level))
            {
                if(x.NoDecay) continue;
                if (!guild.Roles.TryGetValue(x.Role, out var role)) continue;
                if (currentRoles.Contains(x.Role) && guild.HierarchyCheck(currentUser, role))
                    currentRoles.Remove(x.Role);
            }
            
            await user.ModifyAsync(x => x.RoleIds = currentRoles);
        }

        /// <summary>
        /// Creates a list of all roles the user should be having
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guild"></param>
        /// <param name="level"></param>
        /// <param name="roleList"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private IEnumerable<IRole> GetRoles(IGuildEntity user, IGuild guild, int level, IEnumerable<LevelReward> roleList, bool stack)
        {
            var roles = new List<IRole>();
            ulong role = 0;
            var currentUser = _bot.GetGuild(user.GuildId).GetCurrentUser();

            foreach (var x in roleList)
            {
                role = CreateRoleList(guild, level, stack, x, currentUser, roles, role);
            }
            
            if (stack) return roles;
            if (role == 0) return roles;
            if (guild.Roles.TryGetValue(role, out var getSingleRole) && guild.HierarchyCheck(currentUser, getSingleRole)) 
                roles.Add(getSingleRole);
            return roles;
        }
        
        private static ulong CreateRoleList(IGuild guild, int level, bool stack, LevelReward x, IMember currentUser,
            ICollection<IRole> roles, ulong role)
        {
            if (!guild.Roles.TryGetValue(x.Role, out var getRole)) return role;
            if (stack)
            {
                if (level < x.Level) return role;
                if (guild.HierarchyCheck(currentUser, getRole)) roles.Add(getRole);
            }
            else
            {
                if (level >= x.Level && x.Stackable && guild.HierarchyCheck(currentUser, getRole)) roles.Add(getRole);
                if (level >= x.Level) role = x.Role;
            }

            return role;
        }
    }
}