using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Hanekawa.Extensions.Embed
{
    public static class EmbedExtension
    {
        // Reply from channel
        public static Task<RestUserMessage> ReplyAsync(this IMessageChannel channel, string content, Color colour) =>
            channel.SendEmbedAsync(new LocalEmbedBuilder().Create(content, colour));

        public static Task<RestUserMessage> ReplyAsync(this IMessageChannel channel, LocalEmbedBuilder embed) =>
            channel.SendEmbedAsync(embed);

        // Creates default embed - used here
        public static LocalEmbedBuilder Create(this LocalEmbedBuilder embed, string content, Color colour)
        {
            embed.Color = colour;
            embed.Description = content;
            return embed;
        }
        
        private static Task<RestUserMessage> SendEmbedAsync(this IMessageChannel channel, LocalEmbedBuilder embed) =>
            channel.SendMessageAsync(null, false, embed.Build());

        private static Task<RestUserMessage> SendEmbedAsync(this IMessageChannel channel, LocalEmbedBuilder embed,
            string content) =>
            channel.SendMessageAsync(content, false, embed.Build());
    }
}