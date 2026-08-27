using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Hanekawa.Localize;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests.Mediatr.Warnings;

public class WarningReceivedTests
{
    [Fact]
    public async Task WarningReceived_PersistsValidWarning_AndReturnsLocalizedMessage()
    {
        var mockSet = new List<Warning>().AsQueryable().BuildMockDbSet();
        Warning? stored = null;
        mockSet.Setup(x => x.AddAsync(It.IsAny<Warning>(), It.IsAny<CancellationToken>()))
            .Callback<Warning, CancellationToken>((warning, _) => stored = warning)
            .Returns(ValueTask.FromResult<EntityEntry<Warning>>(null!));

        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).Returns(mockSet.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var member = new DiscordMember
        {
            Id = 10,
            Username = "user",
            Guild = new Guild { GuildId = 1, Name = "guild" }
        };
        var sut = new WarningReceivedHandler(db.Object);

        var result = await sut.HandleAsync(
            new WarningReceived(member, "spam", 42),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Format(Localization.WarnedUser, member.Mention), result.Value.Content);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(stored);
        Assert.Equal(1ul, stored.GuildId);
        Assert.Equal(10ul, stored.UserId);
        Assert.Equal(42ul, stored.ModeratorId);
        Assert.Equal("spam", stored.Reason);
        Assert.True(stored.Valid);
        Assert.NotEqual(Guid.Empty, stored.Id);
    }
}
