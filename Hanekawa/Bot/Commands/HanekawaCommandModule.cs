using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Commands
{
    public abstract class HanekawaCommandModule : DiscordModuleBase<HanekawaCommandContext>
    {
        protected async Task<IUserMessage> ReplyAndDeleteAsync(string message, Color color, TimeSpan? timeout = null) =>
            await ReplyAndDeleteAsync(new LocalMessage().Create(message, color), timeout);

        protected async Task<IUserMessage> ReplyAndDeleteAsync(LocalMessage localMessage, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(25);
            var message = await Context.Channel.SendMessageAsync(localMessage).ConfigureAwait(false);
            try
            {
                _ = Task.Delay(timeout.Value)
                    .ContinueWith(async _ => await message.DeleteAsync().ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch
            {
                // Ignore
            }
            return message;
        }
        
        protected DiscordCommandResult Response(string content, Color color, LocalAllowedMentions mentions = null) =>
            Response(new LocalMessage
            {
                Embed = new LocalEmbed
                {
                    Color = color,
                    Description = content
                },
                AllowedMentions = mentions
            });

        protected DiscordCommandResult Reply(string message, Color color)
            => Reply(new LocalEmbed().CreateDefaultEmbed(message, color));

        protected DiscordResponseCommandResult Reply(LocalMessage builder)
        {
            var result = Response(builder.WithReply(Context.Message.Id, Context.ChannelId, Context.GuildId));
            return result;
        }
    }
}