using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services;
using Hanekawa.Application.Handlers.Services.Activity;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Tests.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class MessageRateLimitPipelineTests
{
    private static readonly DateTimeOffset MessageTime =
        new(2026, 9, 17, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task MessageReceived_InvokesExperienceAndActivity_WhenRateLimitPasses()
    {
        var rateLimiter = new Mock<IMessageRateLimiter>();
        rateLimiter.Setup(x => x.TryPassAsync(1, TestUsers.TestMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var levelService = new Mock<ILevelService>();
        var activityService = new Mock<IActivityService>();
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Activity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton(rateLimiter.Object);
        services.AddSingleton(levelService.Object);
        services.AddSingleton(activityService.Object);
        services.AddSingleton(moduleService.Object);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["expLower"] = "1", ["expUpper"] = "2" })
            .Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<INotificationHandler<MessageReceived>, MessageRateLimitHandler>();
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, MessageReceivedExperienceHandler>();
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, WeeklyActivityHandler>();

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        await using var scope = provider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler<MessageReceived>>();
        var notification = new MessageReceived(1, 2, TestUsers.TestMember, 3, "hello", MessageTime);

        await handler.HandleAsync(notification, CancellationToken.None);

        levelService.Verify(x => x.AddExperienceAsync(TestUsers.TestMember, It.IsAny<int>()), Times.Once);
        activityService.Verify(x => x.TrackMessageAsync(1, TestUsers.TestMember.Id, MessageTime,
            It.IsAny<CancellationToken>()), Times.Once);
        activityService.Verify(x => x.SyncCurrentWeekRolesAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MessageReceived_SkipsDownstreamHandlers_WhenRateLimited()
    {
        var rateLimiter = new Mock<IMessageRateLimiter>();
        rateLimiter.Setup(x => x.TryPassAsync(1, TestUsers.TestMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var levelService = new Mock<ILevelService>();
        var activityService = new Mock<IActivityService>();
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Activity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton(rateLimiter.Object);
        services.AddSingleton(levelService.Object);
        services.AddSingleton(activityService.Object);
        services.AddSingleton(moduleService.Object);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["expLower"] = "1", ["expUpper"] = "2" })
            .Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<INotificationHandler<MessageReceived>, MessageRateLimitHandler>();
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, MessageReceivedExperienceHandler>();
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, WeeklyActivityHandler>();

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        await using var scope = provider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler<MessageReceived>>();
        var notification = new MessageReceived(1, 2, TestUsers.TestMember, 3, "hello", MessageTime);

        await handler.HandleAsync(notification, CancellationToken.None);

        levelService.Verify(x => x.AddExperienceAsync(TestUsers.TestMember, It.IsAny<int>()), Times.Never);
        activityService.Verify(x => x.TrackMessageAsync(It.IsAny<ulong>(), It.IsAny<ulong>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        activityService.Verify(x => x.SyncCurrentWeekRolesAsync(It.IsAny<ulong>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void MessageRateLimitPassedHandlers_ResolveTogether_FromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton(Mock.Of<IMessageRateLimiter>());
        services.AddSingleton(Mock.Of<ILevelService>());
        services.AddSingleton(Mock.Of<IActivityService>());
        services.AddSingleton(Mock.Of<IModuleService>());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, MessageReceivedExperienceHandler>();
        services.AddScoped<INotificationHandler<MessageRateLimitPassed>, WeeklyActivityHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handlers = scope.ServiceProvider.GetServices<INotificationHandler<MessageRateLimitPassed>>().ToList();

        Assert.Equal(2, handlers.Count);
        Assert.Contains(handlers, h => h is MessageReceivedExperienceHandler);
        Assert.Contains(handlers, h => h is WeeklyActivityHandler);
    }
}
