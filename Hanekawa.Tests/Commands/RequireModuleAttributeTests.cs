using System.Reflection;
using System.Runtime.CompilerServices;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Hanekawa.Tests.Commands;

public class RequireModuleAttributeTests
{
    private const ulong GuildId = 100;
    private const ulong OwnerId = 42;
    private const ulong NonOwnerId = 99;

    [Fact]
    public async Task CheckAsync_ReturnsSuccess_WhenAuthorIsBotOwner_WithoutCheckingModule()
    {
        var modules = new Mock<IModuleService>();
        var context = CreateContext(OwnerId, modules.Object, [OwnerId]);

        var result = await new RequireModuleAttribute(ModuleName.Level).CheckAsync(context);

        Assert.True(result.IsSuccessful);
        modules.Verify(
            x => x.IsEnabledAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAsync_ReturnsSuccess_WhenModuleIsEnabled()
    {
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(GuildId, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var context = CreateContext(NonOwnerId, modules.Object, []);

        var result = await new RequireModuleAttribute(ModuleName.Level).CheckAsync(context);

        Assert.True(result.IsSuccessful);
        modules.Verify(x => x.IsEnabledAsync(GuildId, ModuleName.Level, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAsync_ReturnsFailure_WhenModuleIsDisabled()
    {
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(GuildId, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var context = CreateContext(NonOwnerId, modules.Object, []);

        var result = await new RequireModuleAttribute(ModuleName.Level).CheckAsync(context);

        Assert.False(result.IsSuccessful);
        Assert.Equal($"The {ModuleName.Level} module is disabled in this server.", result.FailureReason);
    }

    private static IDiscordGuildCommandContext CreateContext(
        ulong authorId,
        IModuleService moduleService,
        ulong[] ownerIds)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => moduleService);
        var provider = services.BuildServiceProvider();

        var context = new Mock<IDiscordGuildCommandContext>();
        context.SetupGet(x => x.Bot).Returns(CreateBot(ownerIds));
        context.SetupGet(x => x.AuthorId).Returns(new Snowflake(authorId));
        context.SetupGet(x => x.GuildId).Returns(new Snowflake(GuildId));
        context.SetupGet(x => x.Services).Returns(provider);
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    private static DiscordBotBase CreateBot(IEnumerable<ulong> ownerIds)
    {
        var bot = (DiscordBotBase)RuntimeHelpers.GetUninitializedObject(typeof(DiscordBot));
        var field = typeof(DiscordBotBase).GetField(
            "<OwnerIds>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(bot, ownerIds.Select(id => new Snowflake(id)).ToArray());
        return bot;
    }
}
