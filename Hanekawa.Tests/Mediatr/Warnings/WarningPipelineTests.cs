using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Pipelines;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests.Mediatr.Warnings;

public class WarningPipelineTests
{
    [Fact]
    public void IPipelineHandler_Is_IRequestHandler()
    {
        Assert.True(typeof(IRequestHandler<WarningReceived, Response<Message>>)
            .IsAssignableFrom(typeof(IPipelineHandler<WarningReceived, Response<Message>>)));
        Assert.True(typeof(IRequestHandler<WarningReceived, Response<Message>>)
            .IsAssignableFrom(typeof(WarningAdded)));
    }

    [Fact]
    public async Task WarningAdded_CanDecorateHandler_AndInvokesInner()
    {
        var inner = new Mock<IRequestHandler<WarningReceived, Response<Message>>>();
        var expected = new Response<Message>(new Message("warned"));
        inner.Setup(x => x.HandleAsync(It.IsAny<WarningReceived>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(new List<GuildConfig>().AsQueryable().BuildMockDbSet().Object);
        db.Setup(x => x.Warnings).Returns(new List<Warning>().AsQueryable().BuildMockDbSet().Object);

        IRequestHandler<WarningReceived, Response<Message>> sut =
            new WarningAdded(inner.Object, db.Object, Mock.Of<IBot>());

        var request = new WarningReceived(
            new DiscordMember { Guild = new Guild { GuildId = 1 }, Username = "user" },
            "spam",
            2);

        var result = await sut.HandleAsync(request, CancellationToken.None);

        Assert.Same(expected, result);
        inner.Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WarningAdded_DoesNotMute_WhenWarningCountIsBelowThreshold()
    {
        var (sut, bot, request) = CreateSutWithWarnings(
            maxWarnings: 3,
            validRecentCount: 2);

        await sut.HandleAsync(request, CancellationToken.None);

        bot.Verify(x => x.MuteAsync(
            It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task WarningAdded_MutesUser_WhenValidRecentWarningsReachThreshold()
    {
        var (sut, bot, request) = CreateSutWithWarnings(
            maxWarnings: 3,
            validRecentCount: 3);

        await sut.HandleAsync(request, CancellationToken.None);

        bot.Verify(x => x.MuteAsync(
                request.User.Guild.GuildId,
                request.User.Id,
                "Auto-mod warning threshold reached (3)",
                TimeSpan.FromHours(2)),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(7, 3)]
    public async Task WarningAdded_MutesWithFractionalDuration_WhenWarningCountIsNotDivisibleByThree(
        int validRecentCount, int maxWarnings)
    {
        // Regression test: `warningCount / 3` must use floating-point division. Integer
        // division silently truncated the result to 0 for any warningCount of 1 or 2,
        // producing a zero-length (no-op) mute and, more generally, a shorter-than-configured
        // auto-mute duration for any warningCount not evenly divisible by 3.
        var (sut, bot, request) = CreateSutWithWarnings(maxWarnings, validRecentCount);
        var expectedDuration = TimeSpan.FromHours(2 * (validRecentCount / 3.0));

        await sut.HandleAsync(request, CancellationToken.None);

        Assert.True(expectedDuration > TimeSpan.Zero);
        bot.Verify(x => x.MuteAsync(
                request.User.Guild.GuildId,
                request.User.Id,
                $"Auto-mod warning threshold reached ({maxWarnings})",
                expectedDuration),
            Times.Once);
    }

    [Fact]
    public async Task WarningAdded_IgnoresInvalidAndOldWarnings_WhenCountingTowardMute()
    {
        var inner = new Mock<IRequestHandler<WarningReceived, Response<Message>>>();
        inner.Setup(x => x.HandleAsync(It.IsAny<WarningReceived>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response<Message>(new Message("warned")));

        var member = new DiscordMember
        {
            Id = 10,
            Username = "user",
            Guild = new Guild { GuildId = 1, Name = "guild" }
        };

        var warnings = new List<Warning>
        {
            new() { GuildId = 1, UserId = 10, Valid = true, CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { GuildId = 1, UserId = 10, Valid = false, CreatedAt = DateTimeOffset.UtcNow },
            new() { GuildId = 1, UserId = 10, Valid = true, CreatedAt = DateTimeOffset.UtcNow.AddDays(-8) },
            new() { GuildId = 2, UserId = 10, Valid = true, CreatedAt = DateTimeOffset.UtcNow },
            new() { GuildId = 1, UserId = 99, Valid = true, CreatedAt = DateTimeOffset.UtcNow }
        };

        var config = new GuildConfig
        {
            GuildId = 1,
            AdminConfig = new AdminConfig { GuildId = 1, MaxWarnings = 2 }
        };

        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(new List<GuildConfig> { config }.AsQueryable().BuildMockDbSet().Object);
        db.Setup(x => x.Warnings).Returns(warnings.AsQueryable().BuildMockDbSet().Object);

        var bot = new Mock<IBot>();
        IRequestHandler<WarningReceived, Response<Message>> sut =
            new WarningAdded(inner.Object, db.Object, bot.Object);

        await sut.HandleAsync(new WarningReceived(member, "spam", 2), CancellationToken.None);

        bot.Verify(x => x.MuteAsync(
            It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task WarningAdded_DoesNotMute_WhenMaxWarningsIsUnset()
    {
        var (sut, bot, request) = CreateSutWithWarnings(
            maxWarnings: 0,
            validRecentCount: 3);

        await sut.HandleAsync(request, CancellationToken.None);

        bot.Verify(x => x.MuteAsync(
            It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task WarningAdded_DoesNotMute_WhenAdminConfigIsMissing()
    {
        var inner = new Mock<IRequestHandler<WarningReceived, Response<Message>>>();
        inner.Setup(x => x.HandleAsync(It.IsAny<WarningReceived>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response<Message>(new Message("warned")));

        var member = new DiscordMember
        {
            Id = 10,
            Username = "user",
            Guild = new Guild { GuildId = 1, Name = "guild" }
        };

        var warnings = new List<Warning>
        {
            new() { GuildId = 1, UserId = 10, Valid = true, CreatedAt = DateTimeOffset.UtcNow }
        };

        var config = new GuildConfig { GuildId = 1, AdminConfig = null };

        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(new List<GuildConfig> { config }.AsQueryable().BuildMockDbSet().Object);
        db.Setup(x => x.Warnings).Returns(warnings.AsQueryable().BuildMockDbSet().Object);

        var bot = new Mock<IBot>();
        IRequestHandler<WarningReceived, Response<Message>> sut =
            new WarningAdded(inner.Object, db.Object, bot.Object);

        await sut.HandleAsync(new WarningReceived(member, "spam", 2), CancellationToken.None);

        bot.Verify(x => x.MuteAsync(
            It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    private static (IRequestHandler<WarningReceived, Response<Message>> Sut, Mock<IBot> Bot, WarningReceived Request)
        CreateSutWithWarnings(int maxWarnings, int validRecentCount)
    {
        var inner = new Mock<IRequestHandler<WarningReceived, Response<Message>>>();
        inner.Setup(x => x.HandleAsync(It.IsAny<WarningReceived>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response<Message>(new Message("warned")));

        var member = new DiscordMember
        {
            Id = 10,
            Username = "user",
            Guild = new Guild { GuildId = 1, Name = "guild" }
        };

        var warnings = Enumerable.Range(0, validRecentCount)
            .Select(_ => new Warning
            {
                GuildId = 1,
                UserId = 10,
                Valid = true,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
            })
            .ToList();

        var config = new GuildConfig
        {
            GuildId = 1,
            AdminConfig = new AdminConfig { GuildId = 1, MaxWarnings = maxWarnings }
        };

        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(new List<GuildConfig> { config }.AsQueryable().BuildMockDbSet().Object);
        db.Setup(x => x.Warnings).Returns(warnings.AsQueryable().BuildMockDbSet().Object);

        var bot = new Mock<IBot>();
        bot.Setup(x => x.MuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        IRequestHandler<WarningReceived, Response<Message>> sut =
            new WarningAdded(inner.Object, db.Object, bot.Object);

        return (sut, bot, new WarningReceived(member, "spam", 2));
    }
}
