using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class MessageRateLimitHandlerTests
{
    private readonly Mock<IMessageRateLimiter> _rateLimiter = new();
    private readonly Mock<IEventPublisher> _publisher = new();
    private readonly MessageRateLimitHandler _sut;

    public MessageRateLimitHandlerTests()
        => _sut = new MessageRateLimitHandler(_rateLimiter.Object, _publisher.Object,
            Mock.Of<ILogger<MessageRateLimitHandler>>());

    private static MessageReceived CreateNotification()
        => new(1, 2, TestUsers.TestMember, 3, "hello", DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_PublishesMessageRateLimitPassed_WhenRateLimitPasses()
    {
        _rateLimiter.Setup(x => x.TryPassAsync(1, TestUsers.TestMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var notification = CreateNotification();

        await _sut.HandleAsync(notification, CancellationToken.None);

        _publisher.Verify(x => x.PublishAsync(
            It.Is<MessageRateLimitPassed>(m =>
                m.GuildId == notification.GuildId &&
                m.ChannelId == notification.ChannelId &&
                m.MessageId == notification.MessageId &&
                m.Member == notification.Member),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DropsMessage_WhenRateLimited()
    {
        _rateLimiter.Setup(x => x.TryPassAsync(1, TestUsers.TestMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.HandleAsync(CreateNotification(), CancellationToken.None);

        _publisher.Verify(x => x.PublishAsync(It.IsAny<MessageRateLimitPassed>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
