using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Extensions.Embed
{
    public static class EmbedExtension
    {
        // Reply from channel
        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content, uint color) =>
            channel.SendEmbedAsync(new EmbedBuilder().Create(content, color));

        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, string content) =>
            channel.SendEmbedAsync(new EmbedBuilder().Create(content));

        public static Task<IUserMessage> ReplyAsync(this IMessageChannel channel, EmbedBuilder embed) =>
            channel.SendEmbedAsync(embed);
            /*
        // Reply from command context
        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, string content, uint color) =>
            context.Channel.SendEmbedAsync(new EmbedBuilder().Create(content, new Color(color)));

        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, string content) =>
            context.Channel.SendEmbedAsync(new EmbedBuilder().Create(content,
                new Color().GetDefaultColor(context.Guild.Id)));

        public static Task<IUserMessage> ReplyAsync(this HanekawaContext context, EmbedBuilder embed) =>
            context.Channel.SendEmbedAsync(embed);
*/
        // Create default embed - used outside of this class
        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content, uint colour) =>
            context.Create(content, colour);

        public static EmbedBuilder CreateDefault(this EmbedBuilder context, string content) =>
            context.Create(content);

        // Creates default embed - used here
        private static EmbedBuilder Create(this EmbedBuilder embed, string content, uint? colour = null)
        {
            if (colour != null) embed.Color = new Color(colour.Value);
            embed.Description = content;
            return embed;
        }
        
        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder embed) =>
            channel.SendMessageAsync(null, false, embed.Build());

        private static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, EmbedBuilder embed,
            string content) =>
            channel.SendMessageAsync(content, false, embed.Build());
    }
}