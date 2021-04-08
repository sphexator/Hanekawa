using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;

namespace Hanekawa.Extensions
{
    public static class UserExtension
    {
        public static bool HierarchyCheck(this IGuild guild, IMember context, IMember target) 
            => GetHierarchy(guild, context) > GetHierarchy(guild, target);

        public static bool HierarchyCheck(this IGuild guild, IMember context, IRole role)
            => GetHierarchy(guild, context) > role.Position;

        public static CachedGuild GetGuild(this CachedMember context) 
            => context.Client.GetGuild(context.GuildId);

        public static CachedMember GetCurrentUser(this CachedGuild guild) 
            => guild.Client.GetMember(guild.Id, guild.Id);

        /// <summary>
        /// Adds a role to the user (for multiple use modifyAsync)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="role"></param>
        /// <returns>Returns true if success, else false</returns>
        public static async Task<bool> TryAddRoleAsync(this IMember member, IRole role)
        {
            try
            {
                await member.GrantRoleAsync(role.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Removes a role from the user.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="role"></param>
        /// <returns>True if success, else false</returns>
        public static async Task<bool> TryRemoveRoleAsync(this IMember member, IRole role)
        {
            try
            {
                await member.RevokeRoleAsync(role.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to mute a user in VC.
        /// </summary>
        /// <param name="member"></param>
        /// <returns>True if success, else false</returns>
        public static async Task<bool> TryMuteAsync(this IMember member)
        {
            try
            {
                await member.ModifyAsync(x => x.Mute = true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to unmute a user in VC.
        /// </summary>
        /// <param name="member"></param>
        /// <returns>True if success, else false</returns>
        public static async Task<bool> TryUnMute(this IMember member)
        {
            try
            {
                await member.ModifyAsync(x => x.Mute = false);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static int GetHierarchy(IGuild guild, IMember member)
        {
            if (guild.OwnerId == member.Id)
                return int.MaxValue;

            // TODO: account for broken positions?
            var roles = member.GetRoles();
            return roles.Count != 0
                ? roles.Values.Max(x => x.Position)
                : 0;
        }
    }
}