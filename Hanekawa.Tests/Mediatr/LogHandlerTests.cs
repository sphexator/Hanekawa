using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Logs;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Tests.Common;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Mediatr;

public class LogHandlerTests
{
    [Fact]
    public async Task UserBannedHandler_DoesNotThrow_WhenGuildConfigMissing()
    {
        var (sut, bot) = CreateBannedHandler([]);

        await sut.HandleAsync(new UserBanned(TestUsers.TestMember), CancellationToken.None);

        bot.Verify(x => x.GetChannel(It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment?>()),
            Times.Never);
    }

    [Fact]
    public async Task UserBannedHandler_DoesNotThrow_WhenLogConfigMissing()
    {
        var configs = new List<GuildConfig>
        {
            new() { GuildId = 1, LogConfig = null }
        };
        var (sut, bot) = CreateBannedHandler(configs);

        await sut.HandleAsync(new UserBanned(TestUsers.TestMember), CancellationToken.None);

        bot.Verify(x => x.GetChannel(It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment?>()),
            Times.Never);
    }

    [Fact]
    public async Task UserUnbannedHandler_DoesNotThrow_WhenLogConfigMissing()
    {
        var configs = new List<GuildConfig>
        {
            new() { GuildId = 1, LogConfig = null }
        };
        var (sut, bot) = CreateUnbannedHandler(configs);

        await sut.HandleAsync(new UserUnbanned(TestUsers.TestMember), CancellationToken.None);

        bot.Verify(x => x.GetChannel(It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment?>()),
            Times.Never);
    }

    [Fact]
    public async Task UserBannedHandler_SendsEmbed_WhenModLogChannelExists()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = 1,
                LogConfig = new LogConfig { GuildId = 1, ModLogChannelId = 99 }
            }
        };
        var (sut, bot) = CreateBannedHandler(configs);
        bot.Setup(x => x.GetChannel(1, 99)).Returns(99ul);
        bot.Setup(x => x.SendMessageAsync(99, It.IsAny<Embed>(), It.IsAny<Attachment?>()))
            .Returns(Task.CompletedTask);

        await sut.HandleAsync(new UserBanned(TestUsers.TestMember), CancellationToken.None);

        bot.Verify(x => x.SendMessageAsync(99, It.IsAny<Embed>(), It.IsAny<Attachment?>()), Times.Once);
    }

    private static (UserBannedHandler Sut, Mock<IBot> Bot) CreateBannedHandler(List<GuildConfig> configs)
    {
        var bot = new Mock<IBot>();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return (new UserBannedHandler(bot.Object, db.Object, modules.Object), bot);
    }

    private static (UserUnbannedHandler Sut, Mock<IBot> Bot) CreateUnbannedHandler(List<GuildConfig> configs)
    {
        var bot = new Mock<IBot>();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return (new UserUnbannedHandler(bot.Object, db.Object, modules.Object), bot);
    }
}
