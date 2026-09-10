using Hanekawa.Bot.Bot;

namespace Hanekawa.Tests.Bot;

public class ApplicationCommandPathsTests
{
    [Fact]
    public void LocalizationDirectory_UsesBundledApplicationCommandsPath()
    {
        var path = ApplicationCommandPaths.LocalizationDirectory;

        Assert.EndsWith(Path.Combine("application-commands", "localizations"), path);
        Assert.DoesNotContain(Path.GetTempPath(), path);
    }

    [Fact]
    public void CacheDirectory_UsesWritableTempPath()
    {
        var path = ApplicationCommandPaths.CacheDirectory;

        Assert.StartsWith(Path.GetTempPath(), path);
        Assert.EndsWith(Path.Combine("hanekawa", "cache"), path);
    }

    [Fact]
    public void CacheAndLocalizationDirectories_AreDistinct()
    {
        // Regression: pointing both Disqord cache and localizer at the same temp folder
        // silently dropped bundled locale files (PR #166).
        Assert.NotEqual(
            ApplicationCommandPaths.CacheDirectory,
            ApplicationCommandPaths.LocalizationDirectory);
    }

    [Fact]
    public void LocalizationDirectory_ResolvesUnderApplicationBaseDirectory()
    {
        var expected = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "application-commands", "localizations"));
        var actual = Path.GetFullPath(ApplicationCommandPaths.LocalizationDirectory);

        Assert.Equal(expected, actual);
    }
}
