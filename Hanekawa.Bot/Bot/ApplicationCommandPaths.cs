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

    /// <summary>
    /// Returns a localization directory the process can write to. Disqord's localizer
    /// persists locale files via temporary files next to the JSONs, which fails when the
    /// bundled directory is read-only (e.g. non-root Docker containers). In that case the
    /// bundled files are copied next to the writable cache and that copy is used instead.
    /// </summary>
    internal static string ResolveWritableLocalizationDirectory()
    {
        if (IsWritable(LocalizationDirectory))
            return LocalizationDirectory;

        var fallback = Path.Combine(Path.GetTempPath(), "hanekawa", "localizations");
        Directory.CreateDirectory(fallback);
        foreach (var file in Directory.EnumerateFiles(LocalizationDirectory, "*.json"))
            File.Copy(file, Path.Combine(fallback, Path.GetFileName(file)), overwrite: true);

        return fallback;
    }

    private static bool IsWritable(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var probe = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
