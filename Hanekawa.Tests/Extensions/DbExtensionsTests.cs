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

    [Fact]
    public async Task GetOrCreateUserAsync_HandlesDbUpdateException_ByReturningConcurrentlyInsertedUser()
    {
        var existingList = new List<GuildUser>();
        var users = existingList.MockDbSet();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).Returns(users.Object);

        var concurrentlyCreated = new GuildUser { GuildId = 2, Id = 10, User = new User { Id = 10 } };
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                existingList.Add(concurrentlyCreated);
            })
            .ThrowsAsync(new DbUpdateException("Duplicate key"));

        var result = await db.Object.GetOrCreateUserAsync(2, 10);

        Assert.Same(concurrentlyCreated, result);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_RethrowsDbUpdateException_WhenUserCannotBeFound()
    {
        var (db, _) = CreateDb([]);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Other DB error"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.Object.GetOrCreateUserAsync(2, 10).AsTask());
    }

    [Fact]
    public async Task GetOrCreateUserAsync_SerializesConcurrentCreations_AndReturnsCreatedUser()
    {
        var existingList = new List<GuildUser>();
        var (db, users) = CreateDb(existingList);
        users.Setup(x => x.AddAsync(It.IsAny<GuildUser>(), It.IsAny<CancellationToken>()))
            .Callback<GuildUser, CancellationToken>((u, _) => existingList.Add(u))
            .Returns(ValueTask.FromResult<EntityEntry<GuildUser>>(null!));

        var task1 = db.Object.GetOrCreateUserAsync(3, 99).AsTask();
        var task2 = db.Object.GetOrCreateUserAsync(3, 99).AsTask();

        var results = await Task.WhenAll(task1, task2);

        Assert.NotNull(results[0]);
        Assert.NotNull(results[1]);
        Assert.Equal(results[0].Id, results[1].Id);
    }

    private static (Mock<IDbContext> Db, Mock<DbSet<GuildUser>> Users) CreateDb(List<GuildUser> existing)
    {
        var users = existing.MockDbSet();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).Returns(users.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (db, users);
    }
}
