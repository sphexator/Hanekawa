using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandContext : DiscordGuildCommandContext
    {
        public IServiceScope Scope { get; private set; }
        public HanekawaCommandContext(DiscordBotBase bot, IPrefix prefix, IGatewayUserMessage message, CachedTextChannel channel, IServiceScope serviceScope) : base(bot, prefix, message, channel, serviceScope)
        {
            Scope = serviceScope;
        }
    }
}
