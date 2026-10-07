using Hanekawa.Infrastructure.Caches;
using Hanekawa.Interfaces;

namespace Hanekawa.Tests.Infrastructure;

public class CacheKeyProviderTests
{
    private sealed class SampleCached : ICached;

    [Fact]
    public void GetKey_PrefixesTypeName()
    {
        var sut = new CacheKeyProvider<SampleCached>();

        Assert.Equal("SampleCached:inventory_42", sut.GetKey("inventory_42"));
    }
}
