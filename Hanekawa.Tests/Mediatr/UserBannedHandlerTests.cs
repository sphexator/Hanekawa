using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Logs;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Moq;
using Moq.EntityFrameworkCore;
using Color = System.Drawing.Color;

namespace Hanekawa.Tests.Mediatr;

public class UserBannedHandlerTests
{
    private const ulong GuildId = 1;
    private const ulong UserId = 10;
    private const ulong ChannelId = 99;

    [Fact]
    public async Task UserBanned_DoesNotSend_WhenLoggingModuleDisabled()
    {
        var (handler, bot, db) = CreateBannedHandler(enabled: false, configs: []);

        await handler.HandleAsync(CreateBanned(), CancellationToken.None);

        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()), Times.Never);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UserBanned_DoesNotSend_WhenModLogChannelIsNull()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                LogConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = null }
            }
        };
        var (handler, bot, db) = CreateBannedHandler(enabled: true, configs);

        await handler.HandleAsync(CreateBanned(), CancellationToken.None);

        bot.Verify(x => x.GetChannel(It.IsAny<ulong>(), It.IsAny<ulong>()), Times.Never);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()), Times.Never);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UserBanned_ClearsChannel_WhenBotCannotResolveIt()
    {
        var logConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = ChannelId };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, LogConfig = logConfig } };
        var (handler, bot, db) = CreateBannedHandler(enabled: true, configs);
        bot.Setup(x => x.GetChannel(GuildId, ChannelId)).Returns((ulong?)null);

        await handler.HandleAsync(CreateBanned(), CancellationToken.None);

        Assert.Null(logConfig.ModLogChannelId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()), Times.Never);
    }

    [Fact]
    public async Task UserBanned_SendsEmbed_WhenChannelExists()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                LogConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = ChannelId }
            }
        };
        var (handler, bot, _) = CreateBannedHandler(enabled: true, configs);
        bot.Setup(x => x.GetChannel(GuildId, ChannelId)).Returns(ChannelId);

        await handler.HandleAsync(CreateBanned(), CancellationToken.None);

        bot.Verify(x => x.SendMessageAsync(ChannelId, It.Is<Embed>(e =>
            e.Color == Color.Red.ToArgb() &&
            e.Title != null &&
            e.Title.Contains($"{UserId}") &&
            e.Fields.Count == 3), It.IsAny<Attachment>()), Times.Once);
    }

    [Fact]
    public async Task UserUnbanned_ClearsChannel_WhenBotCannotResolveIt()
    {
        var logConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = ChannelId };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, LogConfig = logConfig } };
        var (handler, bot, db) = CreateUnbannedHandler(enabled: true, configs);
        bot.Setup(x => x.GetChannel(GuildId, ChannelId)).Returns((ulong?)null);

        await handler.HandleAsync(CreateUnbanned(), CancellationToken.None);

        Assert.Null(logConfig.ModLogChannelId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()), Times.Never);
    }

    [Fact]
    public async Task UserUnbanned_DoesNotSend_WhenModLogChannelIsNull()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                LogConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = null }
            }
        };
        var (handler, bot, db) = CreateUnbannedHandler(enabled: true, configs);

        await handler.HandleAsync(CreateUnbanned(), CancellationToken.None);

        bot.Verify(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()), Times.Never);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UserUnbanned_SendsEmbed_WhenChannelExists()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                LogConfig = new LogConfig { GuildId = GuildId, ModLogChannelId = ChannelId }
            }
        };
        var (handler, bot, _) = CreateUnbannedHandler(enabled: true, configs);
        bot.Setup(x => x.GetChannel(GuildId, ChannelId)).Returns(ChannelId);

        await handler.HandleAsync(CreateUnbanned(), CancellationToken.None);

        bot.Verify(x => x.SendMessageAsync(ChannelId, It.Is<Embed>(e =>
            e.Color == Color.LimeGreen.ToArgb() &&
            e.Fields.Count == 3), It.IsAny<Attachment>()), Times.Once);
    }

    private static UserBanned CreateBanned()
        => new(new DiscordMember
        {
            Id = UserId,
            Username = "user",
            Guild = new Guild { GuildId = GuildId, Name = "guild" }
        });

    private static UserUnbanned CreateUnbanned()
        => new(new DiscordMember
        {
            Id = UserId,
            Username = "user",
            Guild = new Guild { GuildId = GuildId, Name = "guild" }
        });

    private static (UserBannedHandler Handler, Mock<IBot> Bot, Mock<IDbContext> Db) CreateBannedHandler(
        bool enabled, List<GuildConfig> configs)
    {
        var (bot, db, modules) = CreateDeps(enabled, configs);
        return (new UserBannedHandler(bot.Object, db.Object, modules.Object), bot, db);
    }

    private static (UserUnbannedHandler Handler, Mock<IBot> Bot, Mock<IDbContext> Db) CreateUnbannedHandler(
        bool enabled, List<GuildConfig> configs)
    {
        var (bot, db, modules) = CreateDeps(enabled, configs);
        return (new UserUnbannedHandler(bot.Object, db.Object, modules.Object), bot, db);
    }

    private static (Mock<IBot> Bot, Mock<IDbContext> Db, Mock<IModuleService> Modules) CreateDeps(
        bool enabled, List<GuildConfig> configs)
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<Embed>(), It.IsAny<Attachment>()))
            .Returns(Task.CompletedTask);

        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(GuildId, ModuleName.Logging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enabled);

        return (bot, db, modules);
    }
}
