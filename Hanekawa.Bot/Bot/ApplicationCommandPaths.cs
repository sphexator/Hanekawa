namespace Hanekawa.Bot.Bot;

internal static class ApplicationCommandPaths
{
    private static readonly string ApplicationCommandsRoot =
        Path.Combine(AppContext.BaseDirectory, "application-commands");

    /// <summary>
    /// Writable cache for Disqord command registration state.
    /// </summary>
    internal static string CacheDirectory =>
        Path.Combine(Path.GetTempPath(), "hanekawa", "cache");

    /// <summary>
    /// Bundled slash-command locale files shipped with the bot publish output.
    /// </summary>
    internal static string LocalizationDirectory =>
        Path.Combine(ApplicationCommandsRoot, "localizations");
}
