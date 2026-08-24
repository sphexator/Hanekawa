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
}
