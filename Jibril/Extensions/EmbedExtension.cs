using Discord;
using System.Threading.Tasks;
using Discord.Commands;

namespace Jibril.Extensions
{
    public class EmbedExtension : ModuleBase
    {
        public Task<IUserMessage> SendEmbedAsync(EmbedBuilder GetEmbed) => Context.Channel.SendMessageAsync(string.Empty, embed: GetEmbed.Build());
    }

}