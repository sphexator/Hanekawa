using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Webhook;

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

        public static async Task<IWebhook> GetOrCreateWebhookClientAsync(this ITextChannel channel)
        {
            var currentUser = channel.GetGatewayClient().CurrentUser;
            var webhooks = await channel.FetchWebhooksAsync();
            var client = GetClient(webhooks, currentUser.Id);
            
            if (client != null) return client;
            return await channel.CreateWebhookAsync(currentUser.Name);
        }

        private static IWebhook GetClient(IReadOnlyList<IWebhook> list, Snowflake id)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Creator.Id == id) return list[i];
            }

            return null;
        }
    }
}