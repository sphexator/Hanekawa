using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Service.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandModule : DiscordModuleBase<HanekawaCommandContext>
    {
        // TODO: Add colour service later

        /*
        protected async Task ReplyAsync(string content) => await ReplyAsync(null, false,
            new LocalEmbedBuilder
            {
                Description = content,
                Color = Context.ServiceProvider.GetRequiredService<CacheService>().GetColor(Context.Guild.Id.RawValue)
            }.Build(), LocalMentions.None);

        protected async Task ReplyAsync(LocalEmbedBuilder embed) => await ReplyAsync(null, false, embed.Build(), LocalMentions.None);

        protected async Task ReplyAsync(string content, Color color) => await ReplyAsync(null, false,
            new LocalEmbedBuilder { Description = content, Color = color }.Build(), LocalMentions.None);
            */
    }
}
