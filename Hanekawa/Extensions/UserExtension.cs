using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class UserExtension
    {
        public static string GetName(this IGuildUser user) => user.Nickname ?? user.Username;

        public static string GetAvatar(this IUser user) =>
            user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl();

        public static string GetGame(this IUser user) => user.Activity.Name ?? "N/A";

        public static bool HierarchyCheck(this SocketGuildUser user, SocketGuildUser userNumberTwo) =>
            user.Hierarchy < userNumberTwo.Hierarchy;

        public static bool HierarchyCheck(this SocketGuildUser user, IRole role) => user.Hierarchy < role.Position;
    }
}