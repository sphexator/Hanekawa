using Discord;
using System.Threading.Tasks;

namespace Jibril.Extensions
{
    public static class EmbedExtension 
    {
        public static Task<IUserMessage> SendEmbedAsync(this IMessageChannel ch, EmbedBuilder getEmbed, string content = null)
            => ch.SendMessageAsync(content, embed: getEmbed.Build());
    }
}