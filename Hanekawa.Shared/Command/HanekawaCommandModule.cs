using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;

namespace Hanekawa.Shared.Command
{
    public abstract class HanekawaCommandModule : DiscordModuleBase<HanekawaCommandContext>
    {
        protected async Task<RestUserMessage> ReplyAsync(string content) => await ReplyAsync(null, false,
            new LocalEmbedBuilder {Description = content}.Build(), LocalMentions.None);
    }
}