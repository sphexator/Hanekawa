using System.Globalization;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Activity;
using Hanekawa.Entities.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IDbContext> _db = new();
    private readonly Mock<IBot> _bot = new();
    private readonly ActivityService _sut;

    public ActivityServiceTests()
    {
        _db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new ActivityService(_db.Object, _bot.Object, Mock.Of<ILogger<ActivityService>>());
    }

    private Mock<DbSet<GuildActivity>> SetupActivities(List<GuildActivity> activities)
    {
        var dbSet = activities.MockDbSet();
        _db.Setup(x => x.WeeklyActivities).Returns(dbSet.Object);
        return dbSet;
    }

    private void SetupConfigs(List<GuildConfig> configs)
        => _db.Setup(x => x.GuildConfigs).Returns(configs.MockDbSet().Object);

    [Fact]
    public async Task TrackMessageAsync_CreatesEntry_WhenNoneExistsForWeek()
    {
        var dbSet = SetupActivities([]);
        var timestamp = new DateTimeOffset(2026, 9, 16, 12, 0, 0, TimeSpan.Zero); // Wednesday

        await _sut.TrackMessageAsync(1, 2, timestamp);

        dbSet.Verify(x => x.AddAsync(
            It.Is<GuildActivity>(a =>
                a.GuildId == 1 && a.UserId == 2 &&
                a.WeekStart == new DateOnly(2026, 9, 14) && a.MessageCount == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackMessageAsync_IncrementsExistingEntry_ForSameWeek()
    {
        var existing = new GuildActivity
        {
            GuildId = 1, UserId = 2, WeekStart = new DateOnly(2026, 9, 14), MessageCount = 5
        };
        var dbSet = SetupActivities([existing]);
        var timestamp = new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero); // Friday, same week

        await _sut.TrackMessageAsync(1, 2, timestamp);

        Assert.Equal(6, existing.MessageCount);
        dbSet.Verify(x => x.AddAsync(It.IsAny<GuildActivity>(), It.IsAny<CancellationToken>()), Times.Never);
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackMessageAsync_CreatesNewEntry_WhenWeekDiffers()
    {
        var existing = new GuildActivity
        {
            GuildId = 1, UserId = 2, WeekStart = new DateOnly(2026, 9, 7), MessageCount = 5
        };
        var dbSet = SetupActivities([existing]);
        var timestamp = new DateTimeOffset(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);

        await _sut.TrackMessageAsync(1, 2, timestamp);

        Assert.Equal(5, existing.MessageCount);
        dbSet.Verify(x => x.AddAsync(
            It.Is<GuildActivity>(a => a.WeekStart == new DateOnly(2026, 9, 14)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWeeklyLeaderboardAsync_ReturnsTopUsersForCurrentWeek_OrderedByCount()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 1, WeekStart = currentWeek, MessageCount = 10 },
            new GuildActivity { GuildId = 1, UserId = 2, WeekStart = currentWeek, MessageCount = 50 },
            new GuildActivity { GuildId = 1, UserId = 3, WeekStart = currentWeek, MessageCount = 25 },
            new GuildActivity { GuildId = 1, UserId = 4, WeekStart = previousWeek, MessageCount = 100 },
            new GuildActivity { GuildId = 2, UserId = 5, WeekStart = currentWeek, MessageCount = 100 }
        ]);

        var result = await _sut.GetWeeklyLeaderboardAsync(1);

        Assert.Equal(3, result.Count);
        Assert.Equal([2UL, 3UL, 1UL], result.Select(x => x.UserId).ToArray());
    }

    [Fact]
    public async Task SyncCurrentWeekRolesAsync_DoesNothing_WhenNoRoleConfigured()
    {
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = new ActivityConfig(1) }]);
        SetupActivities([]);

        await _sut.SyncCurrentWeekRolesAsync(1);

        _bot.Verify(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _bot.Verify(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task SyncCurrentWeekRolesAsync_AssignsRoleToTopUsers_AndRemovesFromDropouts()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var config = new ActivityConfig(1)
        {
            CurrentWeekRoleId = 100,
            CurrentWeekTopAmount = 2,
            CurrentHolders = [3]
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 1, WeekStart = currentWeek, MessageCount = 10 },
            new GuildActivity { GuildId = 1, UserId = 2, WeekStart = currentWeek, MessageCount = 5 },
            new GuildActivity { GuildId = 1, UserId = 3, WeekStart = currentWeek, MessageCount = 1 }
        ]);

        await _sut.SyncCurrentWeekRolesAsync(1);

        _bot.Verify(x => x.AddRoleAsync(1, 1, 100), Times.Once);
        _bot.Verify(x => x.AddRoleAsync(1, 2, 100), Times.Once);
        _bot.Verify(x => x.RemoveRoleAsync(1, 3, 100), Times.Once);
        Assert.Equal([1UL, 2UL], config.CurrentHolders);
    }

    [Fact]
    public async Task SyncCurrentWeekRolesAsync_DoesNothing_WhenHoldersAlreadyMatch()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var config = new ActivityConfig(1)
        {
            CurrentWeekRoleId = 100,
            CurrentWeekTopAmount = 1,
            CurrentHolders = [1]
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 1, WeekStart = currentWeek, MessageCount = 10 }
        ]);

        await _sut.SyncCurrentWeekRolesAsync(1);

        _bot.Verify(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _bot.Verify(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_AwardsWinner_Announces_AndResetsCurrentHolders()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            PreviousWeekRoleId = 100,
            PreviousWeekHolderId = 8,
            CurrentWeekRoleId = 200,
            CurrentHolders = [7],
            AnnouncementChannelId = 300,
            AnnouncementMessage = "Week {week}: {user} wins with {count}!",
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 9, WeekStart = previousWeek, MessageCount = 42 },
            new GuildActivity { GuildId = 1, UserId = 5, WeekStart = previousWeek, MessageCount = 10 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.RemoveRoleAsync(1, 8, 100), Times.Once);
        _bot.Verify(x => x.AddRoleAsync(1, 9, 100), Times.Once);
        _bot.Verify(x => x.SendMessageAsync(300,
            $"Week {previousWeek:yyyy-MM-dd}: <@9> wins with 42!", null), Times.Once);
        _bot.Verify(x => x.RemoveRoleAsync(1, 7, 200), Times.Once);
        Assert.Equal(9UL, config.PreviousWeekHolderId);
        Assert.Empty(config.CurrentHolders);
        Assert.Equal(currentWeek, config.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_ProcessesPreviousWeek_OnFirstRun_WhenNeverProcessed()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = null
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 4, WeekStart = previousWeek, MessageCount = 12 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(300,
            It.Is<string>(m => m.Contains("<@4>") && m.Contains("12")), null), Times.Once);
        Assert.Equal(currentWeek, config.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_ProcessesEachPendingGuild_Independently()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var guildOne = new ActivityConfig(1)
        {
            AnnouncementChannelId = 301,
            LastProcessedWeekStart = previousWeek
        };
        var guildTwo = new ActivityConfig(2)
        {
            AnnouncementChannelId = 302,
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs(
        [
            new GuildConfig { GuildId = 1, ActivityConfig = guildOne },
            new GuildConfig { GuildId = 2, ActivityConfig = guildTwo }
        ]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 10, WeekStart = previousWeek, MessageCount = 3 },
            new GuildActivity { GuildId = 2, UserId = 20, WeekStart = previousWeek, MessageCount = 7 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(301,
            It.Is<string>(m => m.Contains("<@10>")), null), Times.Once);
        _bot.Verify(x => x.SendMessageAsync(302,
            It.Is<string>(m => m.Contains("<@20>")), null), Times.Once);
        Assert.Equal(currentWeek, guildOne.LastProcessedWeekStart);
        Assert.Equal(currentWeek, guildTwo.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_AnnouncesWithoutRole_WhenPreviousWeekRoleNotConfigured()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 9, WeekStart = previousWeek, MessageCount = 6 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(300,
            It.Is<string>(m => m.Contains("<@9>")), null), Times.Once);
        _bot.Verify(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _bot.Verify(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_DoesNotBackfill_WhenMultipleWeeksWereMissed()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var missedWeek = currentWeek.AddDays(-14);
        var lastWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = missedWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 10, WeekStart = missedWeek, MessageCount = 5 },
            new GuildActivity { GuildId = 1, UserId = 11, WeekStart = lastWeek, MessageCount = 8 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(300,
            It.Is<string>(m => m.Contains("<@11>") && m.Contains("8")), null), Times.Once);
        _bot.Verify(x => x.SendMessageAsync(300,
            It.Is<string>(m => m.Contains("<@10>")), null), Times.Never);
        Assert.Equal(currentWeek, config.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_SkipsGuildsAlreadyProcessed()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var config = new ActivityConfig(1)
        {
            PreviousWeekRoleId = 100,
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = currentWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities([]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), null), Times.Never);
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_IgnoresGuilds_WithoutActivityConfig()
    {
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = null }]);
        SetupActivities([]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), null), Times.Never);
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncCurrentWeekRolesAsync_TreatsZeroTopAmountAsOne()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var config = new ActivityConfig(1)
        {
            CurrentWeekRoleId = 100,
            CurrentWeekTopAmount = 0,
            CurrentHolders = []
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 1, WeekStart = currentWeek, MessageCount = 10 },
            new GuildActivity { GuildId = 1, UserId = 2, WeekStart = currentWeek, MessageCount = 50 }
        ]);

        await _sut.SyncCurrentWeekRolesAsync(1);

        _bot.Verify(x => x.AddRoleAsync(1, 2, 100), Times.Once);
        _bot.Verify(x => x.AddRoleAsync(1, 1, 100), Times.Never);
        Assert.Equal([2UL], config.CurrentHolders);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_DoesNotReassignPreviousWeekRole_WhenWinnerUnchanged()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            PreviousWeekRoleId = 100,
            PreviousWeekHolderId = 9,
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 9, WeekStart = previousWeek, MessageCount = 42 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        _bot.Verify(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        Assert.Equal(9UL, config.PreviousWeekHolderId);
        Assert.Equal(currentWeek, config.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_AdvancesLastProcessedWeek_WhenPreviousWeekHasNoActivity()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities([]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), null), Times.Never);
        Assert.Equal(currentWeek, config.LastProcessedWeekStart);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_SkipsCurrentWeekRoleCleanup_WhenRoleNotConfigured()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            CurrentHolders = [7, 8],
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities([]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        Assert.Equal([7UL, 8UL], config.CurrentHolders);
    }

    [Fact]
    public async Task SetPreviousWeekRoleAsync_ReturnsDisableMessage_WhenRoleCleared()
    {
        var config = new ActivityConfig(1) { PreviousWeekRoleId = 50 };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        var response = await _sut.SetPreviousWeekRoleAsync(1, null);

        Assert.Null(config.PreviousWeekRoleId);
        Assert.Contains("disabled", response);
    }

    [Fact]
    public async Task SetAnnouncementChannelAsync_ReturnsDisableMessage_WhenChannelCleared()
    {
        var config = new ActivityConfig(1) { AnnouncementChannelId = 400 };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        var response = await _sut.SetAnnouncementChannelAsync(1, null);

        Assert.Null(config.AnnouncementChannelId);
        Assert.Contains("disabled", response);
    }

    [Fact]
    public async Task SetCurrentWeekRoleAsync_ReturnsDisableMessage_WhenRoleCleared()
    {
        var config = new ActivityConfig(1) { CurrentWeekRoleId = 100, CurrentWeekTopAmount = 2 };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        var response = await _sut.SetCurrentWeekRoleAsync(1, null, 2);

        Assert.Null(config.CurrentWeekRoleId);
        Assert.Contains("disabled", response);
    }

    [Fact]
    public async Task SetAnnouncementChannelAsync_CreatesActivityConfig_WhenGuildRowExistsWithoutActivity()
    {
        var guildConfig = new GuildConfig { GuildId = 1, ActivityConfig = null };
        SetupConfigs([guildConfig]);

        var response = await _sut.SetAnnouncementChannelAsync(1, 400);

        Assert.NotNull(guildConfig.ActivityConfig);
        Assert.Equal(400UL, guildConfig.ActivityConfig!.AnnouncementChannelId);
        Assert.Contains("<#400>", response);
    }

    [Fact]
    public async Task ProcessWeekRolloversAsync_UsesDefaultMessage_WhenNoneConfigured()
    {
        var currentWeek = ActivityService.GetWeekStart(DateTimeOffset.UtcNow);
        var previousWeek = currentWeek.AddDays(-7);
        var config = new ActivityConfig(1)
        {
            AnnouncementChannelId = 300,
            LastProcessedWeekStart = previousWeek
        };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);
        SetupActivities(
        [
            new GuildActivity { GuildId = 1, UserId = 9, WeekStart = previousWeek, MessageCount = 3 }
        ]);

        await _sut.ProcessWeekRolloversAsync();

        _bot.Verify(x => x.SendMessageAsync(300,
            "Congratulations <@9> for being the most active user last week with 3 messages!", null), Times.Once);
    }

    [Fact]
    public async Task SetCurrentWeekRoleAsync_CreatesConfig_WhenGuildConfigMissing()
    {
        var configDbSet = new List<GuildConfig>().MockDbSet();
        _db.Setup(x => x.GuildConfigs).Returns(configDbSet.Object);

        var response = await _sut.SetCurrentWeekRoleAsync(1, 100, 3);

        configDbSet.Verify(x => x.AddAsync(
            It.Is<GuildConfig>(g =>
                g.GuildId == 1 &&
                g.ActivityConfig!.CurrentWeekRoleId == 100 &&
                g.ActivityConfig.CurrentWeekTopAmount == 3),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains("top 3", response);
    }

    [Fact]
    public async Task SetCurrentWeekRoleAsync_ClampsTopAmount_ToAtLeastOne()
    {
        var config = new ActivityConfig(1);
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        await _sut.SetCurrentWeekRoleAsync(1, 100, 0);

        Assert.Equal(1, config.CurrentWeekTopAmount);
    }

    [Fact]
    public async Task SetAnnouncementMessageAsync_ResetsToDefault_WhenWhitespace()
    {
        var config = new ActivityConfig(1) { AnnouncementMessage = "custom" };
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        var response = await _sut.SetAnnouncementMessageAsync(1, "   ");

        Assert.Null(config.AnnouncementMessage);
        Assert.Contains("reset", response);
    }

    [Fact]
    public async Task SetAnnouncementMessageAsync_TrimsAndPersistsCustomMessage()
    {
        var config = new ActivityConfig(1);
        SetupConfigs([new GuildConfig { GuildId = 1, ActivityConfig = config }]);

        var response = await _sut.SetAnnouncementMessageAsync(1, "  Hello {user}  ");

        Assert.Equal("Hello {user}", config.AnnouncementMessage);
        Assert.Contains("updated", response);
    }

    [Theory]
    [InlineData("2026-09-14", "2026-09-14")] // Monday stays
    [InlineData("2026-09-16", "2026-09-14")] // Wednesday rolls back
    [InlineData("2026-09-20", "2026-09-14")] // Sunday rolls back to same week
    [InlineData("2026-09-21", "2026-09-21")] // Next Monday is a new week
    public void GetWeekStart_ReturnsMondayOfIsoWeek(string input, string expected)
    {
        var timestamp = DateTimeOffset.Parse(input, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);

        Assert.Equal(DateOnly.Parse(expected), ActivityService.GetWeekStart(timestamp));
    }
}
