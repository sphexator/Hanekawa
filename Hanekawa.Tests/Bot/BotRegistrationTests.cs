using Disqord;
using Disqord.Bot;
using Hanekawa.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Tests.Bot;

public class BotRegistrationTests
{
    [Fact]
    public void IBot_ResolvesToSameInstance_AsHostedDiscordBot()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        services.AddMetrics();
        services.AddSingleton<Token>(Token.Bot("MTIzNDU2Nzg5MDEyMzQ1Njc4.AAAAAA.BBBBBB"));
        services.AddDiscordBot<global::Hanekawa.Bot.Bot.Bot>();

        // Mirrors the registration in Hanekawa.Bot/Program.cs.
        // IBot must forward to the hosted Bot singleton; registering Bot as the
        // implementation type constructs a second instance whose constructor
        // subscribes to InteractionReceived again, double-firing every command.
        services.AddSingleton<IBot>(provider => provider.GetRequiredService<global::Hanekawa.Bot.Bot.Bot>());

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        var hostedBot = provider.GetRequiredService<DiscordBot>();
        var bot = provider.GetRequiredService<IBot>();

        Assert.Same(hostedBot, bot);
    }
}
