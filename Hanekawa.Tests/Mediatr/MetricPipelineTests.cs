using System.Diagnostics.Metrics;
using Hanekawa.Application;
using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Pipelines;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class MetricPipelineTests
{
    [Fact]
    public async Task MetricPipeline_InvokesInnerHandler_AndIncrementsCounter()
    {
        var inner = new Mock<IRequestHandler<WarningReceived, Response<Message>>>();
        var expected = new Response<Message>(new Message("ok"));
        inner.Setup(x => x.HandleAsync(It.IsAny<WarningReceived>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var metrics = new FakeMetrics();

        var sut = new MetricPipeline<WarningReceived, Response<Message>>(
            inner.Object,
            NullLogger<MetricPipeline<WarningReceived, Response<Message>>>.Instance,
            metrics);

        var request = new WarningReceived(
            new Hanekawa.Entities.Discord.DiscordMember
            {
                Guild = new Hanekawa.Entities.Discord.Guild { GuildId = 1 },
                Username = "user"
            },
            "spam",
            2);

        var result = await sut.HandleAsync(request, CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(1, metrics.IncrementCount);
        inner.Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VoidMetricPipeline_InvokesInnerHandler_AndIncrementsCounter()
    {
        var inner = new Mock<IRequestHandler<Ban>>();
        inner.Setup(x => x.HandleAsync(It.IsAny<Ban>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var metrics = new FakeMetrics();

        var sut = new MetricPipeline<Ban>(
            inner.Object,
            NullLogger<MetricPipeline<Ban>>.Instance,
            metrics);

        var request = new Ban { GuildId = 1, UserId = 2, ModeratorId = 3, Reason = "spam" };

        await sut.HandleAsync(request, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        inner.Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecoratedBanHandler_ResolvesAsMetricPipelineFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMetrics>(new FakeMetrics());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IServiceProvider>(sp => sp);
        services.AddDecoratedRequestHandler<Ban, BanHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Ban>>();

        Assert.IsType<MetricPipeline<Ban>>(handler);
    }

    [Fact]
    public void DecoratedWarningHandler_ResolvesAsPipelineFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMetrics>(new FakeMetrics());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(Mock.Of<IDbContext>());
        services.AddSingleton(Mock.Of<IBot>());
        services.AddDecoratedRequestHandler<WarningReceived, Response<Message>, WarningReceivedHandler>(
            typeof(WarningAdded));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<WarningReceived, Response<Message>>>();

        Assert.IsType<WarningAdded>(handler);
    }

    [Fact]
    public async Task DecoratedBanHandler_ResolvesMetricPipeline_AndInvokesBan()
    {
        var metrics = new FakeMetrics();
        var bot = new Mock<IBot>();
        bot.Setup(x => x.BanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot.Object);
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDecoratedRequestHandler<Ban, BanHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Ban>>();
        Assert.IsType<MetricPipeline<Ban>>(handler);

        await handler.HandleAsync(new Ban
        {
            GuildId = 10,
            UserId = 20,
            ModeratorId = 30,
            Reason = "raid",
            Days = 7,
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        bot.Verify(x => x.BanAsync(10, 20, 7, "raid %30%"), Times.Once);
    }

    private sealed class FakeMetrics : IMetrics
    {
        public int IncrementCount { get; private set; }

        public TrackedDuration All<T>(ulong? guildId = null) => Duration();
        public TrackedDuration All(string name, ulong? guildId = null) => Duration();
        public void IncrementCounter<T>(ulong? guildId = null) => IncrementCount++;
        public void IncrementCounter(string name, ulong? guildId = null) => IncrementCount++;
        public TrackedDuration MeasureDuration<T>(ulong? guildId = null) => Duration();
        public TrackedDuration MeasureDuration(string name, ulong? guildId = null) => Duration();

        private static TrackedDuration Duration()
        {
            var meter = new Meter("hanekawa.tests");
            return new TrackedDuration(TimeProvider.System, meter.CreateHistogram<double>("duration"));
        }
    }
}
