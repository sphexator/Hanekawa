using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Logs;
using Hanekawa.Application.Interfaces.Services;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class JoinLeaveHandlerTests
{
    [Fact]
    public async Task UserJoinedHandler_DoesNotThrow()
    {
        var sut = new UserJoinedHandler(new Mock<IModuleService>().Object);
        var notification = new UserJoin(1, 2, "user", "avatar", DateTimeOffset.UtcNow);

        await sut.HandleAsync(notification, CancellationToken.None);
    }

    [Fact]
    public async Task UserLeftHandler_DoesNotThrow()
    {
        var sut = new UserLeftHandler(new Mock<IModuleService>().Object);
        var notification = new UserLeave(1, 2);

        await sut.HandleAsync(notification, CancellationToken.None);
    }
}
