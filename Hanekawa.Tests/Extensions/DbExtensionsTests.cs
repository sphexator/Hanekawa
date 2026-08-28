using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests.Extensions;

public class DbExtensionsTests
{
    [Fact]
    public async Task GetOrCreateUserAsync_ReturnsExistingGuildUser()
    {
        var existing = new GuildUser
        {
            GuildId = 1,
            Id = 10,
            Currency = 50,
            User = new User { Id = 10 }
        };
        var (db, users) = CreateDb([existing]);

        var result = await db.Object.GetOrCreateUserAsync(1, 10);

        Assert.Same(existing, result);
        users.Verify(x => x.AddAsync(It.IsAny<GuildUser>(), It.IsAny<CancellationToken>()), Times.Never);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_CreatesGuildUser_WhenMissing()
    {
        GuildUser? added = null;
        var (db, users) = CreateDb([]);
        users.Setup(x => x.AddAsync(It.IsAny<GuildUser>(), It.IsAny<CancellationToken>()))
            .Callback<GuildUser, CancellationToken>((user, _) => added = user)
            .Returns(ValueTask.FromResult<EntityEntry<GuildUser>>(null!));

        var result = await db.Object.GetOrCreateUserAsync(2, 10);

        Assert.Same(added, result);
        Assert.Equal(2ul, result.GuildId);
        Assert.Equal(10ul, result.Id);
        Assert.Equal(10ul, result.User.Id);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_ReusesGlobalUser_WhenPresentInAnotherGuild()
    {
        var global = new User { Id = 10 };
        var otherGuild = new GuildUser { GuildId = 1, Id = 10, User = global };
        GuildUser? added = null;
        var (db, users) = CreateDb([otherGuild]);
        users.Setup(x => x.AddAsync(It.IsAny<GuildUser>(), It.IsAny<CancellationToken>()))
            .Callback<GuildUser, CancellationToken>((user, _) => added = user)
            .Returns(ValueTask.FromResult<EntityEntry<GuildUser>>(null!));

        var result = await db.Object.GetOrCreateUserAsync(2, 10);

        Assert.Same(added, result);
        Assert.Equal(2ul, result.GuildId);
        Assert.Equal(10ul, result.Id);
        Assert.Same(global, result.User);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static (Mock<IDbContext> Db, Mock<DbSet<GuildUser>> Users) CreateDb(List<GuildUser> existing)
    {
        var users = existing.AsQueryable().BuildMockDbSet();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).Returns(users.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (db, users);
    }
}
