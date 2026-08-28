using System.Text;
using System.Text.Json;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Configs;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Distributed;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class ConfigServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsCachedConfig_WithoutQueryingDatabase()
    {
        var config = new GuildConfig { GuildId = 1, Prefix = "cached." };
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync("1-GuildConfig", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config)));
        var db = new Mock<IDbContext>(MockBehavior.Strict);

        var sut = new ConfigService(cache.Object, db.Object);

        var result = await sut.GetAsync(1);

        Assert.Equal(1ul, result.GuildId);
        Assert.Equal("cached.", result.Prefix);
        db.Verify(x => x.GuildConfigs, Times.Never);
    }

    [Fact]
    public async Task GetAsync_LoadsFromDatabase_AndWritesCache_OnMiss()
    {
        var config = new GuildConfig { GuildId = 1, Prefix = "h." };
        var dbSet = new List<GuildConfig> { config }.BuildMockDbSet();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(dbSet.Object);

        var cache = CreateEmptyCache();
        var sut = new ConfigService(cache.Object, db.Object);

        var result = await sut.GetAsync(1);

        Assert.Same(config, result);
        cache.Verify(x => x.SetAsync(
            "1-GuildConfig",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_CreatesAndPersistsConfig_WhenMissing()
    {
        GuildConfig? added = null;
        var dbSet = new List<GuildConfig>().BuildMockDbSet();
        dbSet.Setup(x => x.AddAsync(It.IsAny<GuildConfig>(), It.IsAny<CancellationToken>()))
            .Callback<GuildConfig, CancellationToken>((config, _) => added = config)
            .Returns(ValueTask.FromResult<EntityEntry<GuildConfig>>(null!));
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(dbSet.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cache = CreateEmptyCache();
        var sut = new ConfigService(cache.Object, db.Object);

        var result = await sut.GetAsync(42);

        Assert.Same(added, result);
        Assert.Equal(42ul, result.GuildId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.SetAsync(
            "42-GuildConfig",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithInclude_LoadsFromDatabase_OnCacheMiss()
    {
        var config = new GuildConfig
        {
            GuildId = 1,
            LogConfig = new LogConfig { GuildId = 1, ModLogChannelId = 9 }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(new List<GuildConfig> { config });
        var cache = CreateEmptyCache();
        var sut = new ConfigService(cache.Object, db.Object);

        var result = await sut.GetAsync(1, typeof(LogConfig));

        Assert.Equal(9ul, result.LogConfig!.ModLogChannelId);
    }

    [Fact]
    public async Task SetAsync_WritesSerializedConfigToCache()
    {
        var cache = CreateEmptyCache();
        var db = new Mock<IDbContext>();
        var sut = new ConfigService(cache.Object, db.Object);
        var config = new GuildConfig { GuildId = 1, Prefix = "x." };

        await sut.SetAsync(1, config);

        cache.Verify(x => x.SetAsync(
            "1-GuildConfig",
            It.Is<byte[]>(bytes => Encoding.UTF8.GetString(bytes).Contains("x.")),
            It.Is<DistributedCacheEntryOptions>(o => o.SlidingExpiration == TimeSpan.FromMinutes(5)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_RemovesGuildConfigCacheKey()
    {
        var cache = CreateEmptyCache();
        var sut = new ConfigService(cache.Object, new Mock<IDbContext>().Object);

        await sut.RemoveAsync<GuildConfig>(1);

        cache.Verify(x => x.RemoveAsync("1-GuildConfig", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<IDistributedCache> CreateEmptyCache()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        cache.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        cache.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return cache;
    }
}
