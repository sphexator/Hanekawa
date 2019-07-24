using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class RoleExtension
    {
        public static async Task<bool> TryAddRoleAsync(this SocketGuildUser user, SocketRole role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (currentUser.GuildPermissions.ManageRoles && currentUser.HierarchyCheck(role))
            {
                await user.AddRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryAddRolesAsync(this SocketGuildUser user, IEnumerable<SocketRole> roles)
        {
            var currentUser = user.Guild.CurrentUser;
            var rolesToAdd = new List<SocketRole>();

            if (!currentUser.GuildPermissions.ManageRoles) return false;
            foreach (var x in roles)
                if (currentUser.HierarchyCheck(x))
                    rolesToAdd.Add(x);
            if (rolesToAdd.Count == 0) return false;

            await user.AddRolesAsync(rolesToAdd);
            return true;
        }

        public static async Task<bool> TryRemoveRoleAsync(this SocketGuildUser user, SocketRole role)
        {
            var currentUser = user.Guild.CurrentUser;
            if (currentUser.GuildPermissions.ManageRoles && currentUser.HierarchyCheck(role))
            {
                await user.RemoveRoleAsync(role);
                return true;
            }

            return false;
        }

        public static async Task<bool> TryRemoveRolesAsync(this SocketGuildUser user, IEnumerable<SocketRole> roles)
        {
            var currentUser = user.Guild.CurrentUser;
            var rolesToRemove = new List<SocketRole>();

            if (!currentUser.GuildPermissions.ManageRoles) return false;
            foreach (var x in roles)
                if (currentUser.HierarchyCheck(x))
                    rolesToRemove.Add(x);
            if (rolesToRemove.Count == 0) return false;

            await user.RemoveRolesAsync(rolesToRemove);
            return true;
        }
    }
}