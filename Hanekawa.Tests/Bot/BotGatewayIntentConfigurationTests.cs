using Disqord.Gateway;
using Hanekawa.Bot.Bot;

namespace Hanekawa.Tests.Bot;

public class BotGatewayIntentConfigurationTests
{
    [Fact]
    public void Configure_IncludesPrivilegedIntentsRequiredByEventHandlers()
    {
        var intents = BotGatewayIntentConfiguration.Configure(GatewayIntents.None);

        Assert.True(intents.HasFlag(GatewayIntents.Members));
        Assert.True(intents.HasFlag(GatewayIntents.MessageContent));
        Assert.True(intents.HasFlag(GatewayIntents.Presences));
    }
}
