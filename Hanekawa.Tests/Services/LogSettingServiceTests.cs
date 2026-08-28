using Hanekawa.Application.Handlers.Commands.Settings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class LogSettingServiceTests
{
    [Fact]
    public async Task SetModLogChannelAsync_DoesNothing_WhenConfigIsMissing()
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(new List<GuildConfig>());
        var sut = new LogSettingService(NullLogger<LogSettingService>.Instance, db.Object);

        await sut.SetModLogChannelAsync(1, 99);

        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetModLogChannelAsync_UpdatesChannel_WhenLogConfigExists()
    {
        var logConfig = new LogConfig { GuildId = 1, ModLogChannelId = 1 };
        var configs = new List<GuildConfig> { new() { GuildId = 1, LogConfig = logConfig } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new LogSettingService(NullLogger<LogSettingService>.Instance, db.Object);

        await sut.SetModLogChannelAsync(1, 99);

        Assert.Equal(99ul, logConfig.ModLogChannelId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetJoinLeaveLogChannelAsync_ClearsChannel_WhenNull()
    {
        var logConfig = new LogConfig { GuildId = 1, JoinLeaveLogChannelId = 5 };
        var configs = new List<GuildConfig> { new() { GuildId = 1, LogConfig = logConfig } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new LogSettingService(NullLogger<LogSettingService>.Instance, db.Object);

        await sut.SetJoinLeaveLogChannelAsync(1, null);

        Assert.Null(logConfig.JoinLeaveLogChannelId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetMessageLogChannelAsync_UpdatesChannel()
    {
        var logConfig = new LogConfig { GuildId = 1 };
        var configs = new List<GuildConfig> { new() { GuildId = 1, LogConfig = logConfig } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new LogSettingService(NullLogger<LogSettingService>.Instance, db.Object);

        await sut.SetMessageLogChannelAsync(1, 7);

        Assert.Equal(7ul, logConfig.MessageLogChannelId);
    }

    [Fact]
    public async Task SetVoiceLogChannelAsync_UpdatesChannel()
    {
        var logConfig = new LogConfig { GuildId = 1 };
        var configs = new List<GuildConfig> { new() { GuildId = 1, LogConfig = logConfig } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new LogSettingService(NullLogger<LogSettingService>.Instance, db.Object);

        await sut.SetVoiceLogChannelAsync(1, 8);

        Assert.Equal(8ul, logConfig.VoiceLogChannelId);
    }
}
