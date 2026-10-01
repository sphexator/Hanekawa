using Hanekawa.Extensions;

namespace Hanekawa.Tests.Extensions;

public class NumberExtensionsTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(999, "999")]
    [InlineData(1000, "1K")]
    [InlineData(1500, "1K")]
    [InlineData(999_999, "999K")]
    [InlineData(1_000_000, "1M")]
    [InlineData(2_000_000_000, "2B")]
    public void Humanize_FormatsCountForDisplay(int value, string expected)
        => Assert.Equal(expected, value.Humanize());
}
