using Disqord.Gateway;
using Hanekawa.Bot.Bot;

namespace Hanekawa.Tests.Bot;

public class BotGatewayIntentConfigurationTests
{
    [Fact]
    public void Parse_All_IncludesPrivilegedIntentsRequiredByEventHandlers()
    {
        var intents = BotGatewayIntentConfiguration.Parse(["All"]);

        Assert.True(intents.HasFlag(GatewayIntents.Members));
        Assert.True(intents.HasFlag(GatewayIntents.MessageContent));
        Assert.True(intents.HasFlag(GatewayIntents.Presences));
    }

    [Fact]
    public void Parse_Unprivileged_ExcludesPrivilegedIntents()
    {
        var intents = BotGatewayIntentConfiguration.Parse(["Unprivileged"]);

        Assert.False(intents.HasFlag(GatewayIntents.Members));
        Assert.False(intents.HasFlag(GatewayIntents.MessageContent));
        Assert.False(intents.HasFlag(GatewayIntents.Presences));
    }

    [Fact]
    public void Parse_NamedFlags_AreCombined()
    {
        var intents = BotGatewayIntentConfiguration.Parse(["Members", "MessageContent", "Presences"]);

        Assert.Equal(
            GatewayIntents.Members | GatewayIntents.MessageContent | GatewayIntents.Presences,
            intents);
    }

    [Fact]
    public void Parse_NullOrEmpty_DefaultsToAll()
    {
        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.Parse(null));
        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.Parse([]));
        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.Parse(["", "  "]));
    }

    [Fact]
    public void Parse_UnknownIntent_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => BotGatewayIntentConfiguration.Parse(["NotAnIntent"]));
        Assert.Contains("NotAnIntent", ex.Message);
    }
}
