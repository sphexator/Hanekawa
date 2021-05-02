using System;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Hanekawa.Bot
{
    public class Hanekawa : DiscordBot
    {
        public Hanekawa(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IPrefixProvider prefixes,
            ICommandQueue queue, CommandService commands, IServiceProvider services, DiscordClient client)
            : base(options, logger, prefixes, queue, commands, services, client)
        { }

        public override DiscordCommandContext CreateCommandContext(IPrefix prefix, IGatewayUserMessage message,
            CachedTextChannel channel)
        {
            var scope = Services.CreateScope();
            var context = message.GuildId != null
                ? new HanekawaCommandContext(this, prefix, message, channel, scope)
                : new DiscordCommandContext(this, prefix, message, scope);
            context.Services.GetRequiredService<ICommandContextAccessor>().Context = context;
            return context;
        }

        protected override LocalMessageBuilder FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            return base.FormatFailureMessage(context, result);
        }
    }
}
