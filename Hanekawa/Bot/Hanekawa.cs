using System;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Commands;
using Hanekawa.Entities.Color;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Hanekawa.Bot
{
    public class Hanekawa : DiscordBot
    {
        public Hanekawa(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IServiceProvider services, 
            DiscordClient client) : base(options, logger, services, client) { }
        
        public override DiscordCommandContext CreateCommandContext(IPrefix prefix, string input,
            IGatewayUserMessage message, CachedTextChannel channel)
        {
            var scope = Services.CreateScope();
            var context = message.GuildId != null
                ? new HanekawaCommandContext(this, prefix, input, message, channel, scope)
                : new DiscordCommandContext(this, prefix, input, message, scope);
            context.Services.GetRequiredService<ICommandContextAccessor>().Context = context;
            return context;
        }

        protected override LocalMessageBuilder FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            var builder = base.FormatFailureMessage(context, result);
            builder.Embed.Color = HanaBaseColor.Bad();
            return builder;
        }
    }
}