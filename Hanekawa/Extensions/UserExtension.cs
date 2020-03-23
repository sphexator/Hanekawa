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
            string result;
            switch (user.Presence.Activity.Type)
            {
                case ActivityType.Listening:
                    result = $"Listening: {user.Presence.Activity.Name}";
                    break;
                case ActivityType.Playing:
                    result = $"Playing: {user.Presence.Activity.Name}";
                    break;
                case ActivityType.Streaming:
                    result = $"Streaming: {user.Presence.Activity.Name}";
                    break;
                case ActivityType.Watching:
                    result = $"Watching: {user.Presence.Activity.Name}";
                    break;
                default:
                    result = "Currently not playing";
                    break;
            }

            return result;
        }

        public static string GetStatus(this CachedUser user)
        {
            string result;
            switch (user.Presence.Status)
            {
                case UserStatus.Online:
                    result = "Online";
                    break;
                case UserStatus.Idle:
                    result = "Idle";
                    break;
                case UserStatus.DoNotDisturb:
                    result = "DND";
                    break;
                case UserStatus.Invisible:
                    result = "Invisible";
                    break;
                case UserStatus.Offline:
                    result = "Offline";
                    break;
                default:
                    result = "N/A";
                    break;
            }

            return result;
        }
    }
}