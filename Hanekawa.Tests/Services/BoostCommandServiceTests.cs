using Hanekawa.Application.Handlers.Commands.Boost;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class BoostCommandServiceTests
{
    [Fact]
    public async Task ListAsync_ReturnsNull_WhenGuildConfigIsMissing()
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(new List<GuildConfig>());

        var sut = new BoostCommands(db.Object);

        var result = await sut.ListAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsNull_WhenBoostConfigIsMissing()
    {
        var configs = new List<GuildConfig>
        {
            new() { GuildId = 1, BoostConfig = null }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);

        var sut = new BoostCommands(db.Object);

        var result = await sut.ListAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsConfig_WhenBoostConfigExists()
    {
        var config = new GuildConfig
        {
            GuildId = 1,
            BoostConfig = new BoostConfig { GuildId = 1, Enabled = true, Experience = 50 }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(new List<GuildConfig> { config });

        var sut = new BoostCommands(db.Object);

        var result = await sut.ListAsync(1);

        Assert.NotNull(result);
        Assert.Same(config, result.Value);
        Assert.True(result.Value.BoostConfig!.Enabled);
        Assert.Equal(50, result.Value.BoostConfig.Experience);
    }
}
