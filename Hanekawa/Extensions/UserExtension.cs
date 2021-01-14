using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Hanekawa.Extensions
{
    public static class UserExtension
    {
        public static bool HierarchyCheck(this CachedMember mainUser, CachedMember comparer) 
            => mainUser.Hierarchy > comparer.Hierarchy;

        public static bool HierarchyCheck(this CachedMember mainUser, CachedRole role) 
            => mainUser.Hierarchy > role.Position;

        public static async Task<bool> TryMute(this CachedMember user)
        {
            if (user.VoiceState == null) return false;
            await user.ModifyAsync(x => x.Mute = true);
            return true;
        }

        public static async Task<bool> TryUnMute(this CachedMember user)
        {
            if (user.VoiceState == null) return false;
            await user.ModifyAsync(x => x.Mute = false);
            return true;
        }

        public static string GetGame(this CachedUser user)
        {
            if (user.Presence.Activity == null) return "Currently not playing";
            var result = user.Presence.Activity.Type switch
            {
                ActivityType.Listening => $"Listening: {user.Presence.Activity.Name}",
                ActivityType.Playing => $"Playing: {user.Presence.Activity.Name}",
                ActivityType.Streaming => $"Streaming: {user.Presence.Activity.Name}",
                ActivityType.Watching => $"Watching: {user.Presence.Activity.Name}",
                _ => "Currently not playing"
            };

            return result;
        }

        public static string GetStatus(this CachedUser user)
        {
            if (user.Presence == null || user.Presence.Status == null) return "N/A";
            var result = user.Presence.Status switch
            {
                UserStatus.Online => "Online",
                UserStatus.Idle => "Idle",
                UserStatus.DoNotDisturb => "DND",
                UserStatus.Invisible => "Invisible",
                UserStatus.Offline => "Offline",
                _ => "N/A"
            };
            return result;
        }

        public static async Task<IMember> GetOrFetchMemberAsync(this CachedGuild guild, Snowflake id)
        {
            IMember user = guild.GetMember(id);
            if (user != null) return user;
            user = await guild.GetMemberAsync(id);
            return user;
        }

        public static async Task<IUser> GetOrFetchUserAsync(this DiscordClientBase client, Snowflake id)
        {
            var user = client.GetUser(id);
            if (user != null) return user;
            var restUser = await client.GetUserAsync(id);
            return restUser;
        }
    }
}