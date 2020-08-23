using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Shared.Command
{
    public abstract class HanekawaCommandModule : DiscordModuleBase<HanekawaCommandContext>
    {
        protected async Task<RestUserMessage> ReplyAsync(string content) => await ReplyAsync(null, false,
            new LocalEmbedBuilder 
            { 
                Description = content, 
                Color = Context.ServiceProvider.GetRequiredService<ColourService>().Get(Context.Guild.Id.RawValue)
            }.Build(), LocalMentions.None);

        protected async Task<RestUserMessage> ReplyAsync(LocalEmbedBuilder embed)
        {
            embed.Color ??= Context.ServiceProvider.GetRequiredService<ColourService>()
                .Get(Context.Guild.Id.RawValue);
            return await ReplyAsync(null, false, embed.Build(), LocalMentions.None);
        }

        protected async Task<RestUserMessage> ReplyAsync(string content, Color color) => await ReplyAsync(null, false,
            new LocalEmbedBuilder { Description = content, Color = color }.Build(), LocalMentions.None);
    } 
}