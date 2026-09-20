using Hanekawa.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hanekawa.Tests.Services;

public class MessageRateLimiterTests
{
    private readonly Mock<IDistributedCache> _cache = new();
    private readonly MessageRateLimiter _sut;

    public MessageRateLimiterTests()
        => _sut = new MessageRateLimiter(_cache.Object, Mock.Of<ILogger<MessageRateLimiter>>());

    [Fact]
    public async Task TryPassAsync_ReturnsTrue_AndSetsCooldown_WhenNoCooldownExists()
    {
        _cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _sut.TryPassAsync(1, 2);

        Assert.True(result);
        _cache.Verify(x => x.SetAsync(
                MessageRateLimiter.Key(1, 2),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o =>
                    o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryPassAsync_ReturnsFalse_WhenCooldownExists()
    {
        _cache.Setup(x => x.GetAsync(MessageRateLimiter.Key(1, 2), It.IsAny<CancellationToken>()))
            .ReturnsAsync("1"u8.ToArray());

        var result = await _sut.TryPassAsync(1, 2);

        Assert.False(result);
        _cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryPassAsync_UsesDistinctKeys_PerGuildAndUser()
    {
        _cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        await _sut.TryPassAsync(1, 2);
        await _sut.TryPassAsync(1, 3);
        await _sut.TryPassAsync(2, 2);

        _cache.Verify(x => x.GetAsync(MessageRateLimiter.Key(1, 2), It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(x => x.GetAsync(MessageRateLimiter.Key(1, 3), It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(x => x.GetAsync(MessageRateLimiter.Key(2, 2), It.IsAny<CancellationToken>()), Times.Once);
    }
}
