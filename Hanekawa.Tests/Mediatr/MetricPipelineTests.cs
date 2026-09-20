using System.Diagnostics.Metrics;
using Hanekawa.Application;
using Hanekawa.Application.Contracts;
using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Moq.EntityFrameworkCore;
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
    public async Task MetricPipeline_VoidHandler_InvokesInnerHandler_AndIncrementsCounter()
    {
        var inner = new Mock<IRequestHandler<Ban>>();
        inner.Setup(x => x.HandleAsync(It.IsAny<Ban>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var metrics = new FakeMetrics();

        var sut = new MetricPipeline<Ban>(
            inner.Object,
            NullLogger<MetricPipeline<Ban>>.Instance,
            metrics);

        var request = new Ban
        {
            GuildId = 1,
            UserId = 2,
            ModeratorId = 3,
            Reason = "spam",
            Days = 1,
            Source = ProviderSource.Discord
        };

        await sut.HandleAsync(request, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        inner.Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
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

    [Fact]
    public async Task DecoratedKickHandler_ResolvesMetricPipeline_AndInvokesKick()
    {
        var metrics = new FakeMetrics();
        var bot = new Mock<IBot>();
        bot.Setup(x => x.KickAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot.Object);
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDecoratedRequestHandler<Kick, KickHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Kick>>();
        Assert.IsType<MetricPipeline<Kick>>(handler);

        await handler.HandleAsync(new Kick
        {
            GuildId = 10,
            UserId = 20,
            ModeratorId = 30,
            Reason = "raid",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        bot.Verify(x => x.KickAsync(10, 20, "raid %30%"), Times.Once);
    }

    [Fact]
    public async Task DecoratedLevelUpHandler_ResolvesMetricPipeline_AndInvokesLevelUp()
    {
        var metrics = new FakeMetrics();
        var levelService = new Mock<ILevelService>();
        var member = new DiscordMember
        {
            Id = 1,
            Guild = new Guild { GuildId = 1 },
            Username = "Bob"
        };
        var config = new Hanekawa.Entities.Configs.GuildConfig { GuildId = 1 };

        var services = new ServiceCollection();
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(levelService.Object);
        services.AddDecoratedRequestHandler<LevelUp, LevelUpRoleHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<LevelUp>>();
        Assert.IsType<MetricPipeline<LevelUp>>(handler);

        await handler.HandleAsync(new LevelUp(member, member.RoleIds, 5, config), CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        levelService.Verify(x => x.AdjustRolesAsync(member, 5, config), Times.Once);
    }

    [Fact]
    public async Task DecoratedWarningClearHandler_ResolvesMetricPipeline_AndInvokesHandler()
    {
        var metrics = new FakeMetrics();
        var user = new DiscordMember
        {
            Id = 10,
            Username = "user",
            Guild = new Guild { GuildId = 1, Name = "guild" }
        };
        var warning = new Warning { GuildId = 1, UserId = 10, Valid = true };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).ReturnsDbSet(new List<Warning> { warning });
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var services = new ServiceCollection();
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(db.Object);
        services.AddDecoratedRequestHandler<WarningClear, Response<Message>, WarningClearHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<WarningClear, Response<Message>>>();
        Assert.IsType<MetricPipeline<WarningClear, Response<Message>>>(handler);

        await handler.HandleAsync(new WarningClear(user, 5, "cleared"), CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        Assert.False(warning.Valid);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecoratedMuteHandler_ResolvesMetricPipeline_AndInvokesMute()
    {
        var metrics = new FakeMetrics();
        var bot = new Mock<IBot>();
        var duration = TimeSpan.FromMinutes(30);
        bot.Setup(x => x.MuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot.Object);
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDecoratedRequestHandler<Mute, MuteHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Mute>>();
        Assert.IsType<MetricPipeline<Mute>>(handler);

        await handler.HandleAsync(new Mute
        {
            GuildId = 10,
            UserId = 20,
            ModeratorId = 30,
            Reason = "spam",
            Duration = duration,
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        bot.Verify(x => x.MuteAsync(10, 20, "spam %30%", duration), Times.Once);
    }

    [Fact]
    public async Task DecoratedUnbanHandler_ResolvesMetricPipeline_AndInvokesUnban()
    {
        var metrics = new FakeMetrics();
        var bot = new Mock<IBot>();
        bot.Setup(x => x.UnbanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot.Object);
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDecoratedRequestHandler<Unban, UnbanHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Unban>>();
        Assert.IsType<MetricPipeline<Unban>>(handler);

        await handler.HandleAsync(new Unban
        {
            GuildId = 10,
            UserId = 20,
            ModeratorId = 30,
            Reason = "appeal",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        bot.Verify(x => x.UnbanAsync(10, 20, "appeal %30%"), Times.Once);
    }

    [Fact]
    public async Task DecoratedUnmuteHandler_ResolvesMetricPipeline_AndInvokesUnmute()
    {
        var metrics = new FakeMetrics();
        var bot = new Mock<IBot>();
        bot.Setup(x => x.UnmuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot.Object);
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddDecoratedRequestHandler<Unmute, UnmuteHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<Unmute>>();
        Assert.IsType<MetricPipeline<Unmute>>(handler);

        await handler.HandleAsync(new Unmute
        {
            GuildId = 10,
            UserId = 20,
            ModeratorId = 30,
            Reason = "expired",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        bot.Verify(x => x.UnmuteAsync(10, 20, "expired %30%"), Times.Once);
    }

    [Fact]
    public async Task DecoratedWarningListHandler_ResolvesMetricPipeline_AndInvokesHandler()
    {
        var metrics = new FakeMetrics();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).ReturnsDbSet(new List<Warning>
        {
            new() { GuildId = 1, UserId = 10, Reason = "spam" }
        });

        var services = new ServiceCollection();
        services.AddSingleton<IMetrics>(metrics);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(db.Object);
        services.AddDecoratedRequestHandler<WarningList, Response<Pagination<Message>>, WarningListHandler>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<WarningList, Response<Pagination<Message>>>>();
        Assert.IsType<MetricPipeline<WarningList, Response<Pagination<Message>>>>(handler);

        var result = await handler.HandleAsync(new WarningList(1, 10), CancellationToken.None);

        Assert.Equal(1, metrics.IncrementCount);
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Items);
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
