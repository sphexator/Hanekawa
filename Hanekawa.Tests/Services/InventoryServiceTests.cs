using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class InventoryServiceTests
{
    private const ulong GuildId = 123;
    private const ulong UserId = 456;

    [Fact]
    public async Task GetInventoryAsync_ReturnsCachedUser()
    {
        var user = CreateUser();
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync($"inventory_{UserId}", It.IsAny<Func<Task<GuildUser>>>()))
            .ReturnsAsync(user);
        var db = new Mock<IDbContext>(MockBehavior.Strict);
        var sut = new InventoryService(db.Object, cache.Object);

        var result = await sut.GetInventoryAsync(GuildId, UserId);

        Assert.Same(user, result);
        db.Verify(x => x.Users, Times.Never);
    }

    [Fact]
    public async Task GetInventoryAsync_LoadsExistingUser_OnCacheMiss()
    {
        var user = CreateUser();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).ReturnsDbSet(new List<GuildUser> { user });
        var sut = new InventoryService(db.Object, CreateFactoryCache());

        var result = await sut.GetInventoryAsync(GuildId, UserId);

        Assert.Same(user, result);
    }

    [Fact]
    public async Task GetInventoryAsync_CreatesUser_WhenMissing()
    {
        GuildUser? added = null;
        var users = new List<GuildUser>().AsQueryable().BuildMockDbSet();
        users.Setup(x => x.AddAsync(It.IsAny<GuildUser>(), It.IsAny<CancellationToken>()))
            .Callback<GuildUser, CancellationToken>((user, _) => added = user)
            .Returns(ValueTask.FromResult<EntityEntry<GuildUser>>(null!));
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).Returns(users.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new InventoryService(db.Object, CreateFactoryCache());

        var result = await sut.GetInventoryAsync(GuildId, UserId);

        Assert.Same(added, result);
        Assert.Equal(GuildId, result.GuildId);
        Assert.Equal(UserId, result.Id);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateInventoryAsync_WithList_ReplacesInventory()
    {
        var existing = CreateUser();
        var inventory = new List<Inventory> { new() { ItemId = Guid.NewGuid(), Amount = 5, UserId = UserId } };
        var (sut, db, cache) = CreateMutatingSut(existing);

        await sut.UpdateInventoryAsync(existing, inventory);

        Assert.Same(inventory, existing.User.Inventory);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.Remove($"inventory_{UserId}"), Times.Once);
    }

    [Fact]
    public async Task UpdateInventoryAsync_WithList_DoesNothing_WhenUserMissing()
    {
        var (sut, db, cache) = CreateMutatingSut([]);

        await sut.UpdateInventoryAsync(CreateUser(), [new Inventory { ItemId = Guid.NewGuid(), Amount = 1 }]);

        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        cache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateInventoryAsync_WithSingleItem_AddsWhenMissing_AndIncrementsWhenPresent()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 2, UserId = UserId });
        var (sut, _, _) = CreateMutatingSut(existing);

        await sut.UpdateInventoryAsync(existing, new Inventory { ItemId = itemId, Amount = 3, UserId = UserId });
        await sut.UpdateInventoryAsync(existing, new Inventory { ItemId = Guid.NewGuid(), Amount = 1, UserId = UserId });

        Assert.Equal(5, existing.User.Inventory.Single(x => x.ItemId == itemId).Amount);
        Assert.Equal(2, existing.User.Inventory.Count);
    }

    [Fact]
    public async Task AddItemAsync_IncreasesAmount_WhenItemExists()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 2, UserId = UserId });
        var (sut, db, cache) = CreateMutatingSut(existing);

        await sut.AddItemAsync(existing, itemId, 3);

        Assert.Equal(5, existing.User.Inventory.Single().Amount);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.Remove($"inventory_{UserId}"), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_AddsNewRow_WhenItemMissing()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser();
        var (sut, _, _) = CreateMutatingSut(existing);

        await sut.AddItemAsync(existing, itemId, 3);

        var added = Assert.Single(existing.User.Inventory);
        Assert.Equal(itemId, added.ItemId);
        Assert.Equal(3, added.Amount);
        Assert.Equal(UserId, added.UserId);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenAmountIsNotPositive()
    {
        var (sut, db, _) = CreateMutatingSut(CreateUser());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.AddItemAsync(CreateUser(), Guid.NewGuid(), 0).AsTask());
        await Assert.ThrowsAsync<ArgumentException>(() => sut.AddItemAsync(CreateUser(), Guid.NewGuid(), -1).AsTask());
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_DoesNothing_WhenUserMissing()
    {
        var (sut, db, cache) = CreateMutatingSut([]);

        await sut.AddItemAsync(CreateUser(), Guid.NewGuid(), 1);

        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        cache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_DecreasesAmount_AndRemovesRowWhenZero()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 5, UserId = UserId });
        var (sut, db, cache) = CreateMutatingSut(existing);

        await sut.RemoveItemAsync(existing, itemId, 3);
        Assert.Equal(2, existing.User.Inventory.Single().Amount);

        await sut.RemoveItemAsync(existing, itemId, 2);
        Assert.Empty(existing.User.Inventory);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        cache.Verify(x => x.Remove($"inventory_{UserId}"), Times.Exactly(2));
    }

    [Fact]
    public async Task RemoveItemAsync_Throws_WhenAmountIsNotPositive()
    {
        var (sut, _, _) = CreateMutatingSut(CreateUser());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.RemoveItemAsync(CreateUser(), Guid.NewGuid(), 0).AsTask());
    }

    [Fact]
    public async Task RemoveItemAsync_Throws_WhenItemIsMissing()
    {
        var existing = CreateUser();
        var (sut, _, _) = CreateMutatingSut(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RemoveItemAsync(existing, Guid.NewGuid(), 1).AsTask());
    }

    [Fact]
    public async Task RemoveItemAsync_Throws_WhenAmountExceedsStock()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 2, UserId = UserId });
        var (sut, _, _) = CreateMutatingSut(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RemoveItemAsync(existing, itemId, 3).AsTask());
    }

    [Fact]
    public async Task HasItemAsync_ReturnsTrue_WhenPresent_OtherwiseFalse()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 2, UserId = UserId });
        var (sut, _, _) = CreateMutatingSut(existing);

        Assert.True(await sut.HasItemAsync(existing, itemId));
        Assert.False(await sut.HasItemAsync(existing, Guid.NewGuid()));
    }

    [Fact]
    public async Task HasItemAsync_ReturnsFalse_WhenUserMissing()
    {
        var (sut, _, _) = CreateMutatingSut([]);

        Assert.False(await sut.HasItemAsync(CreateUser(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetItemCountAsync_ReturnsAmount_WhenPresent()
    {
        var itemId = Guid.NewGuid();
        var existing = CreateUser(new Inventory { ItemId = itemId, Amount = 5, UserId = UserId });
        var (sut, _, _) = CreateMutatingSut(existing);

        Assert.Equal(5, await sut.GetItemCountAsync(UserId, itemId));
    }

    [Fact]
    public async Task GetItemCountAsync_ReturnsZero_WhenUserMissing()
    {
        var (sut, _, _) = CreateMutatingSut([]);

        Assert.Equal(0, await sut.GetItemCountAsync(UserId, Guid.NewGuid()));
    }

    private static GuildUser CreateUser(params Inventory[] inventory)
        => new()
        {
            GuildId = GuildId,
            Id = UserId,
            User = new User { Id = UserId, Inventory = [.. inventory] }
        };

    private static ICacheContext CreateFactoryCache()
    {
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<GuildUser>>>()))
            .Returns((string _, Func<Task<GuildUser>> factory) => new ValueTask<GuildUser>(factory()));
        return cache.Object;
    }

    private static (InventoryService Sut, Mock<IDbContext> Db, Mock<ICacheContext> Cache)
        CreateMutatingSut(GuildUser existing)
        => CreateMutatingSut([existing]);

    private static (InventoryService Sut, Mock<IDbContext> Db, Mock<ICacheContext> Cache)
        CreateMutatingSut(List<GuildUser> users)
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Users).ReturnsDbSet(users);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var cache = new Mock<ICacheContext>();
        return (new InventoryService(db.Object, cache.Object), db, cache);
    }
}
