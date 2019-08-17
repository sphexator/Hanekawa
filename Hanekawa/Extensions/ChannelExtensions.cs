using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Hanekawa.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task<bool> TryApplyPermissionOverwriteAsync(this SocketTextChannel channel, IRole role, OverwritePermissions perms)
        {
            if (!channel.Guild.CurrentUser.GuildPermissions.ManageChannels) return false;
            await channel.AddPermissionOverwriteAsync(role, perms);
            return true;
        }
    }
}