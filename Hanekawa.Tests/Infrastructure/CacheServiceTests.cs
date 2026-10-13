using Hanekawa.Infrastructure.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Hanekawa.Tests.Infrastructure;

public class CacheServiceTests
{
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        var memory = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _sut = new CacheService(memory, NullLogger<CacheService>.Instance);
    }

    [Fact]
    public void Get_ReturnsDefault_WhenKeyIsMissing_ForULong()
    {
        var value = _sut.Get<ulong>("missing-drop-key");

        Assert.Equal(0UL, value);
    }

    [Fact]
    public void AddAndGet_RoundTripsULong_ForDropClaimKeys()
    {
        const ulong channelId = 5;
        const ulong messageId = 99;
        const ulong userId = 10;
        var key = $"{channelId}-{messageId}-drop";

        _sut.Add(key, userId);

        Assert.Equal(userId, _sut.Get<ulong>(key));
    }

    [Fact]
    public void Remove_ClearsCachedValue()
    {
        const string key = "1-2-drop";
        _sut.Add(key, 42UL);

        _sut.Remove(key);

        Assert.Equal(0UL, _sut.Get<ulong>(key));
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsCachedValue_WithoutCallingFactory()
    {
        const string key = "guild-config";
        _sut.Add(key, "cached");
        var factoryCalls = 0;

        var result = await _sut.GetOrCreateAsync(key, () =>
        {
            factoryCalls++;
            return Task.FromResult("new");
        });

        Assert.Equal("cached", result);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateAsync_InvokesFactory_AndCachesResult_WhenMissing()
    {
        const string key = "factory-key";
        var factoryCalls = 0;

        var result = await _sut.GetOrCreateAsync(key, () =>
        {
            factoryCalls++;
            return Task.FromResult("created");
        });

        Assert.Equal("created", result);
        Assert.Equal(1, factoryCalls);
        Assert.Equal("created", _sut.Get<string>(key));
    }

    [Fact]
    public void Add_WithExpiration_StoresAbsoluteTtl()
    {
        var cache = new Mock<IDistributedCache>();
        var sut = new CacheService(cache.Object, NullLogger<CacheService>.Instance);
        var ttl = TimeSpan.FromMinutes(5);

        sut.Add("guild-config", "value", ttl);

        cache.Verify(x => x.Set(
            "guild-config",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(options =>
                options.AbsoluteExpirationRelativeToNow == ttl)));
    }

    [Fact]
    public void Get_WithExpiration_ReturnsStoredValue()
    {
        const string key = "guild-config";
        _sut.Add(key, "cached", TimeSpan.FromMinutes(5));

        Assert.Equal("cached", _sut.Get<string>(key, TimeSpan.FromMinutes(5)));
    }
}
