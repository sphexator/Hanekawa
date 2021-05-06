using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class EvalCommandContext : DiscordGuildCommandContext
    {
        public IServiceScope Scope { get; }
        
        public EvalCommandContext(DiscordBotBase bot, IPrefix prefix, IGatewayUserMessage message,
            CachedTextChannel channel, IServiceScope serviceScope) : base(bot, prefix, message, channel, serviceScope) 
                => Scope = serviceScope;
    }
}