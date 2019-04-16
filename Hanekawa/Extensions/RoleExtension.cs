using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IGuildUser = Discord.IGuildUser;

namespace Hanekawa.Extensions
{
    public static class RoleExtension
    {
        public static async Task<bool> TryAddRoleAsync(this IGuildUser user, IRole role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            if (!(currentUser as SocketGuildUser).HierarchyCheck(role))
            {
                await user.AddRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryAddRolesAsync(this IGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            var result = role.Where(x => !(currentUser as SocketGuildUser).HierarchyCheck(x)).ToList();
            if (result.Count > 0)
            {
                await user.AddRolesAsync(result);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryAddRoleAsync(this SocketGuildUser user, IRole role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            if (!currentUser.HierarchyCheck(role))
            {
                await user.AddRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryAddRolesAsync(this SocketGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            var result = role.Where(x => !currentUser.HierarchyCheck(x)).ToList();
            if (result.Count > 0)
            {
                await user.AddRolesAsync(result);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryRemoveRoleAsync(this SocketGuildUser user, IRole role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            if (!currentUser.HierarchyCheck(role))
            {
                await user.RemoveRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryRemoveRoleAsync(this IGuildUser user, IRole role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            if (!(currentUser as SocketGuildUser).HierarchyCheck(role))
            {
                await user.RemoveRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryRemoveRolesAsync(this IGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            var result = role.Where(x => !(currentUser as SocketGuildUser).HierarchyCheck(x)).ToList();
            if (result.Count > 0)
            {
                await user.RemoveRolesAsync(result);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryRemoveRolesAsync(this SocketGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return false;
            var result = role.Where(x => !currentUser.HierarchyCheck(x)).ToList();
            if (result.Count > 0)
            {
                await user.RemoveRolesAsync(result);
                return true;
            }

            return false;
        }
    }
}