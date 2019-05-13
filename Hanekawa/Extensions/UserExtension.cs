using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class UserExtension
    {
        public static bool HierarchyCheck(this SocketGuildUser mainUser, SocketGuildUser comparer)
        {
            if (mainUser.Hierarchy > comparer.Hierarchy) return true;
            return false;
        }

        public static bool HierarchyCheck(this SocketGuildUser mainUser, SocketRole role)
        {
            if (mainUser.Hierarchy > role.Position) return true;
            return false;
        }

        public static async Task<bool> TryMute(this SocketGuildUser user)
        {
            if (!user.VoiceState.HasValue) return false;
            await user.ModifyAsync(x => x.Mute = true);
            return true;
        }

        public static async Task<bool> TryUnMute(this SocketGuildUser user)
        {
            if (!user.VoiceState.HasValue) return false;
            await user.ModifyAsync(x => x.Mute = false);
            return true;
        }

        public static string GetName(this SocketGuildUser user)
        {
            if (user.Nickname != null) return user.Nickname;
            return user.Username;
        }

        public static string GetAvatar(this SocketUser user)
        {
            var avi = user.GetAvatarUrl(ImageFormat.Auto, 2048);
            if (avi != null) return avi;
            return user.GetDefaultAvatarUrl();
        }

        public static string GetGame(this SocketUser user)
        {
            string result;
            switch (user.Activity.Type)
            {
                case ActivityType.Listening:
                    result = $"Listening: {user.Activity.Name}";
                    break;
                case ActivityType.Playing:
                    result = $"Playing: {user.Activity.Name}";
                    break;
                case ActivityType.Streaming:
                    result = $"Streaming: {user.Activity.Name}";
                    break;
                case ActivityType.Watching:
                    result = $"Watching: {user.Activity.Name}";
                    break;
                default:
                    result = "Currently not playing";
                    break;
            }

            return result;
        }

        public static string GetStatus(this SocketUser user)
        {
            string result;
            switch (user.Status)
            {
                case UserStatus.Online:
                    result = "Online";
                    break;
                case UserStatus.Idle:
                    result = "Idle";
                    break;
                case UserStatus.AFK:
                    result = "AFK";
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