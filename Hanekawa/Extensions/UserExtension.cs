using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class UserExtension
    {
        public static string GetName(this IGuildUser user)
        {
            return user.Nickname ?? user.Username;
        }

        public static string GetAvatar(this IUser user)
        {
            return user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl();
        }

        public static string GetGame(this IUser user)
        {
            return user.Activity.Name ?? "N/A";
        }

        public static bool HierarchyCheck(this SocketGuildUser user, SocketGuildUser userNumberTwo)
        {
            return user.Hierarchy > userNumberTwo.Hierarchy;
        }
    }
}