using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.WebUI.Exceptions;

namespace Hanekawa.WebUI.Extensions
{
    public static class RoleExtension
    {
        public static async Task TryAddRoleAsync(this CachedMember user, CachedRole role)
        {
            if (!user.GetGuild().HierarchyCheck(user.GetGuild().GetCurrentUser(), role))
                throw new HanaCommandException("Can't add a role that's placed higher than the bot");
            await user.GrantRoleAsync(role.Id);
        }

        public static async Task TryAddRolesAsync(this CachedMember user, IEnumerable<CachedRole> roles)
        {
            var guild = user.GetGuild();
            var currentUser = guild.GetCurrentUser();
            if (Discord.Permissions.CalculatePermissions(guild, currentUser, currentUser.GetRoles().Values).ManageRoles)
                throw new HanaCommandException("");

            var finalRoles = user.GetRoles().Keys;
            var snowflakes = new List<Snowflake>(finalRoles);
            foreach (var x in roles)
            {
                if (x == null) continue;
                if (guild.HierarchyCheck(currentUser, x) && !snowflakes.Contains(x.Id)) snowflakes.Add(x.Id);
            }

            await user.ModifyAsync(x => x.RoleIds = snowflakes);
        }

        public static async Task TryRemoveRoleAsync(this CachedMember user, CachedRole role)
        {
            var guild = user.GetGuild();
            var currentUser = guild.GetCurrentUser();
            if (Discord.Permissions.CalculatePermissions(guild, currentUser, currentUser.GetRoles().Values).ManageRoles)
                throw new HanaCommandException("");
            if (guild.HierarchyCheck(currentUser, role)) throw new HanaCommandException("Can't add a role that's placed higher than the bot");
            await user.RevokeRoleAsync(role.Id);
        }

        public static async Task TryRemoveRolesAsync(this CachedMember user, IEnumerable<CachedRole> roles)
        {
            var guild = user.GetGuild();
            var currentUser = guild.GetCurrentUser();
            if (Discord.Permissions.CalculatePermissions(guild, currentUser, currentUser.GetRoles().Values).ManageRoles)
                throw new HanaCommandException("");
            var finalRoles = user.GetRoles().Keys;
            var snowflakes = new List<Snowflake>(finalRoles);
            foreach (var x in roles)
            {
                if (x == null) continue;
                if (guild.HierarchyCheck(currentUser, x) && snowflakes.Contains(x.Id)) snowflakes.Remove(x.Id);
            }

            await user.ModifyAsync(x => x.RoleIds = snowflakes);
        }
    }
}