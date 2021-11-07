using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;

namespace Hanekawa.WebUI.Extensions
{
    public static class UserExtension
    {
        /// <summary>
        /// Check hierarchy between two users. Returns true if user is below context hierarchy.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool HierarchyCheck(this IMember context, IMember target)
        {
            var guild = context.GetGuild();
            return GetHierarchy(guild, context) > GetHierarchy(guild, target);
        }

        /// <summary>
        /// Check hierarchy between a user and a role. Returns true if role is below context hierarchy.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool HierarchyCheck(this IMember context, IRole role)
        {
            var guild = context.GetGuild();
            return GetHierarchy(guild, context) > role.Position;
        }
        
        /// <summary>
        /// Check hierarchy between a user and a role. Returns true if user is below your hierarchy.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="context"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool HierarchyCheck(this IGuild guild, IMember context, IRole role)
            => GetHierarchy(guild, context) > role.Position;

        /// <summary>
        /// Check hierarchy between the bot and a user. Returns true if user is below bot hierarchy.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool HierarchyCheck(this IGuild guild, IMember user)
            => GetHierarchy(guild, guild.GetMember(guild.GetGatewayClient().CurrentUser.Id)) >
               GetHierarchy(guild, user);

        public static string DisplayName(this IMember user) => user.Nick ?? user.Name;
        
        /// <summary>
        /// Get guild from a cached member.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static CachedGuild GetGuild(this CachedMember context) 
            => context.Client.GetGuild(context.GuildId);
        
        /// <summary>
        /// Get current bot user from a guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static CachedMember GetCurrentUser(this CachedGuild guild) 
            => guild.Client.GetMember(guild.Id, guild.Id);
        /// <summary>
        /// Tries to get user from cache, else retrieves from rest
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async ValueTask<IMember> GetOrFetchMemberAsync(this IGuild guild, Snowflake userId)
        {
            var user = guild.GetMember(userId);
            if (user != null) return user;
            return await guild.FetchMemberAsync(userId);
        }
        
        /// <summary>
        /// Tries to get user from cache, else retrieves from rest
        /// </summary>
        /// <param name="client"></param>
        /// <param name="guildId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async ValueTask<IMember> GetOrFetchMemberAsync(this IGatewayClient client, Snowflake guildId,
            Snowflake userId)
        {
            var user = client.GetMember(guildId, userId);
            if (user != null) return user;
            var guild = client.GetGuild(guildId);
            return await guild.FetchMemberAsync(userId);
        }
        
        /// <summary>
        /// Gets as many people as possible from cache, rest from rest
        /// </summary>
        /// <param name="client"></param>
        /// <param name="guildId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static async ValueTask<List<IMember>> GetOrFetchMembersAsync(this IGatewayClient client,
            Snowflake guildId, IEnumerable<Snowflake> userIds)
        {
            var toRetrieve = new List<Snowflake>();
            var users = new List<IMember>();
            foreach (var x in userIds)
            {
                var user = client.GetMember(guildId, x);
                if (user == null) toRetrieve.Add(x);
                else users.Add(user);
            }

            if (toRetrieve.Count == 0) return users;
            var guild = client.GetGuild(guildId);
            users.AddRange(await FetchMembersAsync(guild, toRetrieve));

            return users;
        }
        
        /// <summary>
        /// Gets as many people as possible from cache, rest from rest
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static async ValueTask<List<IMember>> GetOrFetchMembersAsync(this IGuild guild, IEnumerable<Snowflake> userIds)
        {
            var toRetrieve = new List<Snowflake>();
            var users = new List<IMember>();
            foreach (var x in userIds)
            {
                var user = guild.GetMember(x);
                if (user == null) toRetrieve.Add(x);
                else users.Add(user);
            }

            if (toRetrieve.Count == 0) return users;
            users.AddRange(await FetchMembersAsync(guild, toRetrieve));
            return users;
        }

        private static async Task<List<IMember>> FetchMembersAsync(IGuild guild, IEnumerable<Snowflake> userIds)
        {
            var users = new List<IMember>();
            foreach (var x in userIds)
            {
                var user = await guild.FetchMemberAsync(x);
                if (user != null) users.Add(user);
            }

            return users;
        }

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

        private static int GetHierarchy(IGuild guild, IMember member)
        {
            if (guild.OwnerId == member.Id)
                return int.MaxValue;

            var roles = member.GetRoles();
            return roles.Count != 0
                ? roles.Values.Max(x => x.Position)
                : 0;
        }
    }
}