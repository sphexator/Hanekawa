using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Extensions
{
    public static class RoleExtension
    {
        public static async Task TryAddRoleAsync(this IGuildUser user, IRole role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (currentUser.GuildPermissions.ManageRoles)
            {
                await user.AddRoleAsync(role);
            }
        }

        public static async Task TryAddRolesAsync(this IGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (currentUser.GuildPermissions.ManageRoles)
            {
                await user.AddRolesAsync(role);
            }
        }

        public static async Task TryRemoveRoleAsync(this IGuildUser user, IRole role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (currentUser.GuildPermissions.ManageRoles)
            {
                await user.RemoveRoleAsync(role);
            }
        }

        public static async Task TryRemoveRolesAsync(this IGuildUser user, IEnumerable<IRole> role)
        {
            var currentUser = (user.Guild as SocketGuild)?.CurrentUser ?? await user.Guild.GetCurrentUserAsync();
            if (currentUser.GuildPermissions.ManageRoles)
            {
                await user.RemoveRolesAsync(role);
            }
        }
    }
}
