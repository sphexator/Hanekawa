using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;

namespace Hanekawa.Extensions
{
    public static class RoleExtension
    {
        public static async Task<bool> TryAddRoleAsync(this CachedMember user, CachedRole role)
        {
            var currentUser = user.Guild.CurrentMember;
            if (role == null) return false;
            if (!currentUser.Permissions.ManageRoles || !currentUser.HierarchyCheck(role)) return false;
            await user.GrantRoleAsync(role.Id.RawValue);
            return true;

        }

        public static async Task<bool> TryAddRolesAsync(this CachedMember user, IEnumerable<CachedRole> roles)
        {
            var currentUser = user.Guild.CurrentMember;

            if (!currentUser.Permissions.ManageRoles) return false;
            foreach (var x in roles)
            {
                if (x == null) continue;
                if (currentUser.HierarchyCheck(x))
                    await user.GrantRoleAsync(x.Id.RawValue);
            }
            return true;
        }

        public static async Task<bool> TryRemoveRoleAsync(this CachedMember user, CachedRole role)
        {
            var currentUser = user.Guild.CurrentMember;
            if (role == null) return false;
            if (!currentUser.Permissions.ManageRoles || !currentUser.HierarchyCheck(role)) return false;
            await user.RevokeRoleAsync(role.Id.RawValue);
            return true;

        }

        public static async Task<bool> TryRemoveRolesAsync(this CachedMember user, IEnumerable<CachedRole> roles)
        {
            var currentUser = user.Guild.CurrentMember;
            if (!currentUser.Permissions.ManageRoles) return false;
            foreach (var x in roles)
            {
                if (x == null) continue;
                if (currentUser.HierarchyCheck(x))
                    await user.RevokeRoleAsync(x.Id.RawValue);
            }
            return true;
        }
    }
}