using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;

namespace Hanekawa.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task<bool> TryApplyPermissionOverwriteAsync(this CachedTextChannel channel, LocalOverwrite perms)
        {
            var guild = channel.GetGatewayClient().GetGuild(channel.GuildId);
            var currentUser = guild.GetCurrentUser();
            if (!Discord.Permissions.CalculatePermissions(guild, currentUser, currentUser.GetRoles().Values).ManageChannels) return false;
            await channel.SetOverwriteAsync(perms);
            return true;
        }
    }
}