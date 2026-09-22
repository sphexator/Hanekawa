using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hanekawa.Tests.Bot;

public class ActivityRolloverBackgroundServiceTests
{
    [Fact]
    public async Task StartAsync_InvokesProcessWeekRollovers_BeforeFirstTimerTick()
    {
        var invoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var activity = new Mock<IActivityService>();
        activity.Setup(x => x.ProcessWeekRolloversAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                invoked.TrySetResult();
                return Task.CompletedTask;
            });

        var services = new ServiceCollection();
        services.AddScoped(_ => activity.Object);
        var provider = services.BuildServiceProvider();

        var sut = new ActivityRolloverBackgroundService(
            provider,
            Mock.Of<ILogger<ActivityRolloverBackgroundService>>());

        await sut.StartAsync(CancellationToken.None);
        await invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        activity.Verify(x => x.ProcessWeekRolloversAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_DoesNotFault_WhenProcessWeekRolloversThrows()
    {
        var invoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var activity = new Mock<IActivityService>();
        activity.Setup(x => x.ProcessWeekRolloversAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                invoked.TrySetResult();
                throw new InvalidOperationException("db unavailable");
            });

        var services = new ServiceCollection();
        services.AddScoped(_ => activity.Object);
        var provider = services.BuildServiceProvider();

        var sut = new ActivityRolloverBackgroundService(
            provider,
            Mock.Of<ILogger<ActivityRolloverBackgroundService>>());

        await sut.StartAsync(CancellationToken.None);
        await invoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        activity.Verify(x => x.ProcessWeekRolloversAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
