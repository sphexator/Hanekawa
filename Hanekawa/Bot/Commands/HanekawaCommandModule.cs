using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandModule : DiscordModuleBase<HanekawaCommandContext>
    {
        public async Task<IUserMessage> ReplyAndDeleteAsync(string message, Color color, TimeSpan? timeout = null) =>
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(message, color), timeout);
        
        public async Task<IUserMessage> ReplyAndDeleteAsync(LocalMessage localMessage, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(25);
            var message = await Context.Channel.SendMessageAsync(localMessage).ConfigureAwait(false);
            try
            {
                _ = Task.Delay(timeout.Value)
                    .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore
            }
            return message;
        }

        public async Task Reply(LocalMessage localMessageBuilder)
        {
            await Context.Channel.SendMessageAsync(localMessageBuilder);
        }

        public async Task Reply(string message, Color color)
        {
            await Context.Channel.SendMessageAsync(new LocalMessageBuilder().Create(message, color));
        }
    }
}