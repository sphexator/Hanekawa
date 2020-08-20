using System.Threading.Tasks;
using Disqord;

namespace Hanekawa.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task<bool> TryApplyPermissionOverwriteAsync(this CachedTextChannel channel, LocalOverwrite perms)
        {
            if (!channel.Guild.CurrentMember.Permissions.ManageChannels) return false;
            await channel.AddOrModifyOverwriteAsync(perms);
            return true;
        }
    }
}