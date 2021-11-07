using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Entities.Color;
using Hanekawa.WebUI.Bot.Commands;
using Hanekawa.WebUI.Bot.Commands.TypeReaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Hanekawa.WebUI.Bot
{
    public class Hanekawa : DiscordBot
    {
        public Hanekawa(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger,
            IServiceProvider services,
            DiscordClient client) : base(options, logger, services, client)
        { }

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = new ())
        {
            Commands.AddTypeParser(new TimeSpanTypeParser());
            return base.AddTypeParsersAsync(cancellationToken);
        }

        public override DiscordCommandContext CreateCommandContext(IPrefix prefix, string input,
            IGatewayUserMessage message, CachedMessageGuildChannel channel)
        {
            var scope = Services.CreateScope();
            var context = message.GuildId != null
                ? new HanekawaCommandContext(this, prefix, input, message, channel, scope)
                : new DiscordCommandContext(this, prefix, input, message, scope);
            context.Services.GetRequiredService<ICommandContextAccessor>().Context = context;
            return context;
        }

        protected override LocalMessage FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            var builder = base.FormatFailureMessage(context, result);
            foreach (var x in builder.Embeds)
                x.Color = HanaBaseColor.Bad();

            return builder;
        }
    }
}