using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Activity;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities;
using Hanekawa.Tests.Common;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class WeeklyActivityHandlerTests
{
    private readonly Mock<IModuleService> _moduleService = new();
    private readonly Mock<IActivityService> _activityService = new();
    private readonly WeeklyActivityHandler _sut;

    public WeeklyActivityHandlerTests()
        => _sut = new WeeklyActivityHandler(_moduleService.Object, _activityService.Object);

    private static MessageRateLimitPassed CreateNotification()
        => new(1, 2, TestUsers.TestMember, 3, "hello", DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_TracksMessage_WhenActivityModuleEnabled()
    {
        _moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Activity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var notification = CreateNotification();

        await _sut.HandleAsync(notification, CancellationToken.None);

        _activityService.Verify(x => x.TrackMessageAsync(notification.GuildId, notification.Member.Id,
            notification.CreatedAt, It.IsAny<CancellationToken>()), Times.Once);
        _activityService.Verify(x => x.SyncCurrentWeekRolesAsync(notification.GuildId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PassesMessageTimestamp_ToTrackMessageAsync()
    {
        var timestamp = new DateTimeOffset(2026, 9, 17, 8, 15, 0, TimeSpan.Zero);
        _moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Activity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var notification = new MessageRateLimitPassed(1, 2, TestUsers.TestMember, 3, "hello", timestamp);

        await _sut.HandleAsync(notification, CancellationToken.None);

        _activityService.Verify(x => x.TrackMessageAsync(1, TestUsers.TestMember.Id, timestamp,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SkipsTracking_WhenActivityModuleDisabled()
    {
        _moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Activity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.HandleAsync(CreateNotification(), CancellationToken.None);

        _activityService.Verify(x => x.TrackMessageAsync(It.IsAny<ulong>(), It.IsAny<ulong>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        _activityService.Verify(x => x.SyncCurrentWeekRolesAsync(It.IsAny<ulong>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
