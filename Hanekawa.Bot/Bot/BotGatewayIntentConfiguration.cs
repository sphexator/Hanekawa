using Disqord.Gateway;

namespace Hanekawa.Bot.Bot;

internal static class BotGatewayIntentConfiguration
{
    internal const string SectionKey = "Discord:Intents";

    /// <summary>
    /// Privileged members, message content, and presences are required by
    /// <c>DiscordEventRegister</c>. Default to the full intent set when config is missing.
    /// </summary>
    internal static GatewayIntents Default => GatewayIntents.All;

    internal static GatewayIntents FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionKey);
        if (!section.Exists())
            return Default;

        var names = section.Get<string[]>();
        if (names is { Length: > 0 })
            return Parse(names);

        if (!string.IsNullOrWhiteSpace(section.Value))
            return Parse([section.Value]);

        return Default;
    }

    internal static GatewayIntents Parse(IEnumerable<string>? names)
    {
        if (names is null)
            return Default;

        var result = GatewayIntents.None;
        var any = false;
        foreach (var raw in names)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var name = raw.Trim();
            if (!Enum.TryParse<GatewayIntents>(name, ignoreCase: true, out var intent))
            {
                throw new InvalidOperationException(
                    $"Unknown Discord gateway intent '{name}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<GatewayIntents>())}.");
            }

            result |= intent;
            any = true;
        }

        return any ? result : Default;
    }
}
