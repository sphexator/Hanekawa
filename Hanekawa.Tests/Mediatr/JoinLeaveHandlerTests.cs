using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Logs;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class JoinLeaveHandlerTests
{
    [Fact]
    public async Task UserJoinedHandler_ChecksLoggingModule()
    {
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(1, ModuleName.Logging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var sut = new UserJoinedHandler(modules.Object);

        await sut.HandleAsync(new UserJoin(1, 2, "user", "avatar", DateTimeOffset.UtcNow), CancellationToken.None);

        modules.Verify(x => x.IsEnabledAsync(1, ModuleName.Logging, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserLeftHandler_ChecksLoggingModule()
    {
        var modules = new Mock<IModuleService>();
        modules.Setup(x => x.IsEnabledAsync(1, ModuleName.Logging, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = new UserLeftHandler(modules.Object);

        await sut.HandleAsync(new UserLeave(1, 2), CancellationToken.None);

        modules.Verify(x => x.IsEnabledAsync(1, ModuleName.Logging, It.IsAny<CancellationToken>()), Times.Once);
    }
}
