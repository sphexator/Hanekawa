using Discord;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Jibril.Extensions
{
    public static class EmbedExtension 
    {
        public static Task<IUserMessage> SendEmbedAsync(IMessageChannel ch, EmbedBuilder getEmbed)
            => ch.SendMessageAsync(string.Empty, embed: getEmbed.Build());
    }

}