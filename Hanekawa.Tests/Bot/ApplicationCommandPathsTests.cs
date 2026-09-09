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
}
