using Disqord.Gateway;

namespace Hanekawa.Bot.Bot;

internal static class BotGatewayIntentConfiguration
{
    internal static GatewayIntents Configure(GatewayIntents current)
        => current | GatewayIntents.All;
}
