using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class AdministrationHandlerTests
{
    [Fact]
    public async Task BanHandler_AppendsModeratorIdToReason()
    {
        var (handler, bot) = CreateBanHandler();

        await handler.HandleAsync(new Ban
        {
            GuildId = 1,
            UserId = 10,
            ModeratorId = 42,
            Reason = "spam",
            Days = 3,
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        bot.Verify(x => x.BanAsync(1, 10, 3, "spam %42%"), Times.Once);
    }

    [Fact]
    public async Task KickHandler_AppendsModeratorIdToReason()
    {
        var (handler, bot) = CreateKickHandler();

        await handler.HandleAsync(new Kick
        {
            GuildId = 1,
            UserId = 10,
            ModeratorId = 42,
            Reason = "spam",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        bot.Verify(x => x.KickAsync(1, 10, "spam %42%"), Times.Once);
    }

    [Fact]
    public async Task MuteHandler_AppendsModeratorIdAndForwardsDuration()
    {
        var (handler, bot) = CreateMuteHandler();
        var duration = TimeSpan.FromHours(2);

        await handler.HandleAsync(new Mute
        {
            GuildId = 1,
            UserId = 10,
            ModeratorId = 42,
            Reason = "spam",
            Duration = duration,
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        bot.Verify(x => x.MuteAsync(1, 10, "spam %42%", duration), Times.Once);
    }

    [Fact]
    public async Task UnbanHandler_AppendsModeratorIdToReason()
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.UnbanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var handler = new UnbanHandler(CreateKeyedProvider(bot.Object));

        await handler.HandleAsync(new Unban
        {
            GuildId = 1,
            UserId = 10,
            ModeratorId = 42,
            Reason = "appeal",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        bot.Verify(x => x.UnbanAsync(1, 10, "appeal %42%"), Times.Once);
    }

    [Fact]
    public async Task UnmuteHandler_AppendsModeratorIdToReason()
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.UnmuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var handler = new UnmuteHandler(CreateKeyedProvider(bot.Object));

        await handler.HandleAsync(new Unmute
        {
            GuildId = 1,
            UserId = 10,
            ModeratorId = 42,
            Reason = "expired",
            Source = ProviderSource.Discord
        }, CancellationToken.None);

        bot.Verify(x => x.UnmuteAsync(1, 10, "expired %42%"), Times.Once);
    }

    private static (BanHandler Handler, Mock<IBot> Bot) CreateBanHandler()
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.BanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return (new BanHandler(CreateKeyedProvider(bot.Object)), bot);
    }

    private static (KickHandler Handler, Mock<IBot> Bot) CreateKickHandler()
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.KickAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return (new KickHandler(CreateKeyedProvider(bot.Object)), bot);
    }

    private static (MuteHandler Handler, Mock<IBot> Bot) CreateMuteHandler()
    {
        var bot = new Mock<IBot>();
        bot.Setup(x => x.MuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        return (new MuteHandler(CreateKeyedProvider(bot.Object)), bot);
    }

    private static ServiceProvider CreateKeyedProvider(IBot bot)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IBot>(nameof(ProviderSource.Discord), bot);
        return services.BuildServiceProvider();
    }
}
