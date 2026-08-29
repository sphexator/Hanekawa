using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Tests.Services;

public class ItemServiceTests
{
    [Fact]
    public async Task GetItemByIdAsync_ReturnsCachedItem()
    {
        var itemId = Guid.NewGuid();
        var item = CreateItem(itemId, "Potion", "consumable");
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync($"item_{itemId}", It.IsAny<Func<Task<Item?>>>()))
            .ReturnsAsync(item);
        var db = new Mock<IDbContext>(MockBehavior.Strict);
        var sut = new ItemService(db.Object, cache.Object);

        var result = await sut.GetItemByIdAsync(itemId);

        Assert.Same(item, result);
        db.Verify(x => x.Items, Times.Never);
    }

    [Fact]
    public async Task GetItemByIdAsync_LoadsFromDatabase_OnCacheMiss()
    {
        var itemId = Guid.NewGuid();
        var item = CreateItem(itemId, "Potion", "consumable");
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Items).ReturnsDbSet(new List<Item> { item });
        var sut = new ItemService(db.Object, CreateFactoryCache<Item?>());

        var result = await sut.GetItemByIdAsync(itemId);

        Assert.Same(item, result);
        Assert.Equal("consumable", result!.Type.Name);
    }

    [Fact]
    public async Task GetItemByNameAsync_MatchesCaseInsensitively_WithoutUsingCache()
    {
        var item = CreateItem(Guid.NewGuid(), "Dragon Scale", "collectible");
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Items).ReturnsDbSet(new List<Item> { item });
        var cache = new Mock<ICacheContext>(MockBehavior.Strict);
        var sut = new ItemService(db.Object, cache.Object);

        var result = await sut.GetItemByNameAsync("dragon scale");

        Assert.Same(item, result);
    }

    [Fact]
    public async Task GetItemByNameAsync_ReturnsNull_WhenMissing()
    {
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Items).ReturnsDbSet(new List<Item>());
        var sut = new ItemService(db.Object, new Mock<ICacheContext>().Object);

        var result = await sut.GetItemByNameAsync("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllItemsAsync_LoadsItems_OnCacheMiss()
    {
        var item = CreateItem(Guid.NewGuid(), "Potion", "consumable");
        var db = new Mock<IDbContext>();
        db.Setup(x => x.Items).ReturnsDbSet(new List<Item> { item });
        var sut = new ItemService(db.Object, CreateFactoryCache<List<Item>>());

        var result = (await sut.GetAllItemsAsync()).ToList();

        Assert.Same(item, Assert.Single(result));
    }

    [Fact]
    public async Task CreateItemAsync_Throws_WhenTypeIsMissing()
    {
        var typeId = Guid.NewGuid();
        var types = new List<ItemType>().AsQueryable().BuildMockDbSet();
        types.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((ItemType?)null);
        var db = new Mock<IDbContext>();
        db.Setup(x => x.ItemTypes).Returns(types.Object);
        var sut = new ItemService(db.Object, new Mock<ICacheContext>().Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.CreateItemAsync("Sword", "sharp", typeId).AsTask());

        Assert.Equal("typeId", ex.ParamName);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateItemAsync_PersistsItem_AndInvalidatesAllItemsCache()
    {
        var typeId = Guid.NewGuid();
        var itemType = new ItemType { Id = typeId, Name = "consumable" };
        Item? added = null;
        var types = new List<ItemType> { itemType }.AsQueryable().BuildMockDbSet();
        types.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(itemType);
        var items = new List<Item>().AsQueryable().BuildMockDbSet();
        items.Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
            .Callback<Item, CancellationToken>((item, _) => added = item)
            .Returns(ValueTask.FromResult<EntityEntry<Item>>(null!));
        var cache = new Mock<ICacheContext>();
        var db = new Mock<IDbContext>();
        db.Setup(x => x.ItemTypes).Returns(types.Object);
        db.Setup(x => x.Items).Returns(items.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new ItemService(db.Object, cache.Object);

        var result = await sut.CreateItemAsync("Potion", "heals", typeId, 25);

        Assert.Same(added, result);
        Assert.Equal("Potion", result.Name);
        Assert.Equal("heals", result.Description);
        Assert.Equal(typeId, result.TypeId);
        Assert.Equal(25, result.Price);
        Assert.NotEqual(Guid.Empty, result.Id);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.Remove("all_items"), Times.Once);
    }

    [Fact]
    public async Task UseItemAsync_Throws_WhenItemIsMissing()
    {
        var itemId = Guid.NewGuid();
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync($"item_{itemId}", It.IsAny<Func<Task<Item?>>>()))
            .ReturnsAsync((Item?)null);
        var sut = new ItemService(new Mock<IDbContext>().Object, cache.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UseItemAsync(1, 10, itemId).AsTask());

        Assert.Equal("Item not found", ex.Message);
    }

    [Theory]
    [InlineData("consumable")]
    [InlineData("Collectible")]
    [InlineData("UTILITY")]
    public async Task UseItemAsync_Succeeds_ForKnownTypes(string typeName)
    {
        var itemId = Guid.NewGuid();
        var item = CreateItem(itemId, "Thing", typeName);
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync($"item_{itemId}", It.IsAny<Func<Task<Item?>>>()))
            .ReturnsAsync(item);
        var sut = new ItemService(new Mock<IDbContext>().Object, cache.Object);

        await sut.UseItemAsync(1, 10, itemId);
    }

    [Fact]
    public async Task UseItemAsync_Throws_WhenTypeHasNoImplementation()
    {
        var itemId = Guid.NewGuid();
        var item = CreateItem(itemId, "Sword", "weapon");
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync($"item_{itemId}", It.IsAny<Func<Task<Item?>>>()))
            .ReturnsAsync(item);
        var sut = new ItemService(new Mock<IDbContext>().Object, cache.Object);

        var ex = await Assert.ThrowsAsync<NotSupportedException>(() =>
            sut.UseItemAsync(1, 10, itemId).AsTask());

        Assert.Contains("weapon", ex.Message);
    }

    private static Item CreateItem(Guid id, string name, string typeName)
        => new()
        {
            Id = id,
            Name = name,
            Description = "desc",
            TypeId = Guid.NewGuid(),
            Type = new ItemType { Id = Guid.NewGuid(), Name = typeName }
        };

    private static ICacheContext CreateFactoryCache<T>()
    {
        var cache = new Mock<ICacheContext>();
        cache.Setup(x => x.GetOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<T>>>()))
            .Returns((string _, Func<Task<T>> factory) => new ValueTask<T>(factory()));
        return cache.Object;
    }
}
