using System.Text;

namespace Hanekawa.Tests.Bot;

public class BotBanTests
{
    [Fact]
    public void BanAsync_UsesSingleUserBanEndpoint()
    {
        var assemblyPath = typeof(global::Hanekawa.Bot.Bot.Bot).Assembly.Location;
        var utf8 = Encoding.UTF8.GetString(File.ReadAllBytes(assemblyPath));

        // CreateBansAsync reports per-user failures in the response and does not throw,
        // so BanAsync must keep using the single-user endpoint.
        Assert.Contains("CreateBanAsync", utf8, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateBansAsync", utf8, StringComparison.Ordinal);
    }
}
