using System.Text;
using Disqord.Gateway;
using Hanekawa.Bot.Bot;
using Microsoft.Extensions.Configuration;

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

    [Fact]
    public void FromConfiguration_MissingSection_DefaultsToAll()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.FromConfiguration(configuration));
    }

    [Fact]
    public void FromConfiguration_ArrayBinding_IncludesPrivilegedIntentsRequiredByEventHandlers()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Intents:0"] = "All"
            })
            .Build();

        var intents = BotGatewayIntentConfiguration.FromConfiguration(configuration);

        Assert.True(intents.HasFlag(GatewayIntents.Members));
        Assert.True(intents.HasFlag(GatewayIntents.MessageContent));
        Assert.True(intents.HasFlag(GatewayIntents.Presences));
    }

    [Fact]
    public void FromConfiguration_JsonArrayBinding_MatchesAppsettingsShape()
    {
        const string json = """
            {
              "Discord": {
                "Intents": [ "All" ]
              }
            }
            """;

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        var intents = BotGatewayIntentConfiguration.FromConfiguration(configuration);

        Assert.Equal(GatewayIntents.All, intents);
    }

    [Fact]
    public void FromConfiguration_ScalarValue_IncludesPrivilegedIntents()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Intents"] = "All"
            })
            .Build();

        var intents = BotGatewayIntentConfiguration.FromConfiguration(configuration);

        Assert.Equal(GatewayIntents.All, intents);
    }

    [Fact]
    public void FromConfiguration_EmptyArrayOrWhitespace_DefaultsToAll()
    {
        const string emptyArrayJson = """
            {
              "Discord": {
                "Intents": []
              }
            }
            """;

        var emptyArray = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(emptyArrayJson)))
            .Build();

        var whitespaceOnly = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Intents:0"] = "  "
            })
            .Build();

        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.FromConfiguration(emptyArray));
        Assert.Equal(GatewayIntents.All, BotGatewayIntentConfiguration.FromConfiguration(whitespaceOnly));
    }

    [Fact]
    public void FromConfiguration_UnknownIntent_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Intents:0"] = "NotAnIntent"
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(
            () => BotGatewayIntentConfiguration.FromConfiguration(configuration));
        Assert.Contains("NotAnIntent", ex.Message);
    }

    [Fact]
    public void ShippedAppsettings_BindsDiscordIntentsWithPrivilegedFlagsRequiredByEventHandlers()
    {
        var appsettingsPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "Hanekawa.Bot", "appsettings.json"));
        Assert.True(File.Exists(appsettingsPath), $"Expected shipped appsettings at {appsettingsPath}");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath)
            .Build();

        var intents = BotGatewayIntentConfiguration.FromConfiguration(configuration);

        Assert.True(intents.HasFlag(GatewayIntents.Members));
        Assert.True(intents.HasFlag(GatewayIntents.MessageContent));
        Assert.True(intents.HasFlag(GatewayIntents.Presences));
    }
}
