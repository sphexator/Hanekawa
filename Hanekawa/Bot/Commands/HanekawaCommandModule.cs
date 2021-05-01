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
        protected async Task<IUserMessage> ReplyAndDeleteAsync(string message, Color color, TimeSpan? timeout = null) =>
            await ReplyAndDeleteAsync(new LocalMessageBuilder().Create(message, color), timeout);

        protected async Task<IUserMessage> ReplyAndDeleteAsync(LocalMessage localMessage, TimeSpan? timeout = null)
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

        protected override ValueTask AfterExecutedAsync()
        {
            Context.Scope.Dispose();
            return base.AfterExecutedAsync();
        }

        protected async Task Reply(LocalMessage localMessageBuilder) 
            => await Context.Channel.SendMessageAsync(localMessageBuilder);

        protected async Task Reply(string message, Color color) 
            => await Context.Channel.SendMessageAsync(new LocalMessageBuilder().Create(message, color));
    }
}