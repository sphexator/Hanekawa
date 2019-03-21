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
            if (user.Activity.Name != null) return user.Activity.Name;
            return "N/A";
        }
    }
}