using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Hanekawa.Localize;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Mediatr.Warnings;

public class WarningClearHandlerTests
{
    private readonly DiscordMember _user = new()
    {
        Id = 10,
        Username = "user",
        Guild = new Guild { GuildId = 1, Name = "guild" }
    };

    [Fact]
    public async Task ClearAll_InvalidatesOnlyValidWarningsForUser()
    {
        var keepValid = new Warning { GuildId = 1, UserId = 10, Valid = true };
        var alreadyInvalid = new Warning { GuildId = 1, UserId = 10, Valid = false };
        var otherUser = new Warning { GuildId = 1, UserId = 99, Valid = true };
        var otherGuild = new Warning { GuildId = 2, UserId = 10, Valid = true };
        var warnings = new List<Warning> { keepValid, alreadyInvalid, otherUser, otherGuild };

        var db = CreateDb(warnings);
        var sut = new WarningClearHandler(db.Object, NullLogger<WarningClearHandler>.Instance);

        var result = await sut.HandleAsync(
            new WarningClear(_user, 5, "cleared", All: true),
            CancellationToken.None);

        Assert.False(keepValid.Valid);
        Assert.False(alreadyInvalid.Valid);
        Assert.True(otherUser.Valid);
        Assert.True(otherGuild.Valid);
        Assert.Equal(string.Format(Localization.ClearedAllWarnUserMention, _user.Mention), result.Value.Content);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearSingle_InvalidatesFirstValidWarning()
    {
        var first = new Warning { GuildId = 1, UserId = 10, Valid = true, Reason = "one" };
        var second = new Warning { GuildId = 1, UserId = 10, Valid = true, Reason = "two" };
        var warnings = new List<Warning> { first, second };

        var db = CreateDb(warnings);
        var sut = new WarningClearHandler(db.Object, NullLogger<WarningClearHandler>.Instance);

        var result = await sut.HandleAsync(
            new WarningClear(_user, 5, "voided"),
            CancellationToken.None);

        Assert.False(first.Valid);
        Assert.True(second.Valid);
        Assert.Equal(string.Format(Localization.ClearedWarningUserMention, _user.Mention), result.Value.Content);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearSingle_ReturnsNoWarnings_WhenNoneAreValid()
    {
        var warnings = new List<Warning>
        {
            new() { GuildId = 1, UserId = 10, Valid = false }
        };

        var db = CreateDb(warnings);
        var sut = new WarningClearHandler(db.Object, NullLogger<WarningClearHandler>.Instance);

        var result = await sut.HandleAsync(
            new WarningClear(_user, 5, "voided"),
            CancellationToken.None);

        Assert.Equal(string.Format(Localization.NoWarningsUserMention, _user.Mention), result.Value.Content);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IDbContext> CreateDb(List<Warning> warnings)
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Warnings).ReturnsDbSet(warnings);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }
}
