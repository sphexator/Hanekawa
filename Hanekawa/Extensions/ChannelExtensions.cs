using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
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

        public static async ValueTask<IUserMessage> GetOrFetchMessageAsync(this ITextChannel channel, Snowflake id)
        {
            var cache = channel.GetGatewayClient().CacheProvider;
            if(cache.TryGetMessages(channel.Id, out var messageCache))
            {
                if(messageCache.TryGetValue(id, out var cachedUserMessage)) return cachedUserMessage;
            }

            return await channel.FetchMessageAsync(id) as IUserMessage;
        }
    }
}