using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandContext : DiscordGuildCommandContext
    {
        public IServiceScope Scope { get; }

        public HanekawaCommandContext(DiscordBotBase bot, IPrefix prefix, string input, IGatewayUserMessage message, 
            CachedTextChannel channel, IServiceScope serviceScope) : base(bot, prefix, input, message, channel, serviceScope) =>
            Scope = serviceScope;
    }
}