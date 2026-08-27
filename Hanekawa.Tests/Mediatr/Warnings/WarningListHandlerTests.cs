using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Mediatr.Warnings;

public class WarningListHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsNoWarnings_WhenUserHasNone()
    {
        var warnings = new List<Warning>
        {
            new() { GuildId = 1, UserId = 99, Reason = "other user" },
            new() { GuildId = 2, UserId = 10, Reason = "other guild" }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).ReturnsDbSet(warnings);

        var sut = new WarningListHandler(db.Object);
        var result = await sut.HandleAsync(new WarningList(1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("No warnings found", result.Value.Items.Single().Content);
    }

    [Fact]
    public async Task HandleAsync_ReturnsWarningsForGuildAndUser()
    {
        var warnings = new List<Warning>
        {
            new() { GuildId = 1, UserId = 10, Reason = "spam" },
            new() { GuildId = 1, UserId = 99, Reason = "other user" }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).ReturnsDbSet(warnings);

        var sut = new WarningListHandler(db.Object);
        var result = await sut.HandleAsync(new WarningList(1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Items);
        Assert.DoesNotContain(result.Value.Items, x => x.Content == "No warnings found");
    }
}
