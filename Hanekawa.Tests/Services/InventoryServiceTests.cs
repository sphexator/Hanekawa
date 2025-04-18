using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hanekawa.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IDbContext> _mockDbContext;
        private readonly Mock<ICacheContext> _mockCache;
        private readonly IInventoryService _inventoryService;
        private readonly Mock<DbSet<GuildUser>> _mockUserDbSet;

        public InventoryServiceTests()
        {
            _mockDbContext = new Mock<IDbContext>();
            _mockCache = new Mock<ICacheContext>();
            _mockUserDbSet = new Mock<DbSet<GuildUser>>();

            _mockDbContext.Setup(db => db.Users).Returns(_mockUserDbSet.Object);

            _inventoryService = new InventoryService(_mockDbContext.Object, _mockCache.Object);
        }

        [Fact]
        public async Task GetInventoryAsync_CallsCache_ReturnsUser()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var user = new GuildUser { GuildId = guildId, Id = userId };

            _mockCache.Setup(c => c.GetOrCreateAsync($"inventory_{userId}", It.IsAny<Func<Task<GuildUser>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _inventoryService.GetInventoryAsync(guildId, userId);

            // Assert
            Assert.Equal(userId, result.Id);
            Assert.Equal(guildId, result.GuildId);
            _mockCache.Verify(c => c.GetOrCreateAsync($"inventory_{userId}", It.IsAny<Func<Task<GuildUser>>>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInventoryAsync_WithList_UpdatesUserInventory()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var user = new GuildUser { GuildId = guildId, Id = userId };
            var appUser = new User { Id = userId, Inventory = new List<Inventory>() };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            var inventory = new List<Inventory>
            {
                new() { ItemId = Guid.NewGuid(), Amount = 5, UserId = userId }
            };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.UpdateInventoryAsync(user, inventory);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);
            Assert.Same(inventory, existingUser.User.Inventory);
        }

        [Fact]
        public async Task UpdateInventoryAsync_WithSingleItem_UpdatesUserInventory()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };
            var appUser = new User { Id = userId, Inventory = new List<Inventory>() };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            var inventoryItem = new Inventory { ItemId = itemId, Amount = 5, UserId = userId };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.UpdateInventoryAsync(user, inventoryItem);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);
            Assert.Contains(existingUser.User.Inventory, i => i.ItemId == itemId);
        }

        [Fact]
        public async Task AddItemAsync_ExistingItem_IncreasesAmount()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var existingInventory = new List<Inventory>
            {
                new() { ItemId = itemId, Amount = 2, UserId = userId }
            };
            var appUser = new User { Id = userId, Inventory = existingInventory };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.AddItemAsync(user, itemId, 3);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);

            var updatedItem = existingUser.User.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(5, updatedItem.Amount); // 2 + 3 = 5
        }

        [Fact]
        public async Task AddItemAsync_NewItem_AddsToInventory()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var appUser = new User { Id = userId, Inventory = new List<Inventory>() };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.AddItemAsync(user, itemId, 3);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);

            var addedItem = existingUser.User.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            Assert.NotNull(addedItem);
            Assert.Equal(3, addedItem.Amount);
        }

        [Fact]
        public async Task RemoveItemAsync_SufficientAmount_DecreasesAmount()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var existingInventory = new List<Inventory>
            {
                new() { ItemId = itemId, Amount = 5, UserId = userId }
            };
            var appUser = new User { Id = userId, Inventory = existingInventory };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.RemoveItemAsync(user, itemId, 3);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);

            var updatedItem = existingUser.User.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            Assert.NotNull(updatedItem);
            Assert.Equal(2, updatedItem.Amount); // 5 - 3 = 2
        }

        [Fact]
        public async Task RemoveItemAsync_RemoveAll_RemovesItemFromInventory()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var existingInventory = new List<Inventory>
            {
                new() { ItemId = itemId, Amount = 3, UserId = userId }
            };
            var appUser = new User { Id = userId, Inventory = existingInventory };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            await _inventoryService.RemoveItemAsync(user, itemId, 3);

            // Assert
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.Remove($"inventory_{userId}"), Times.Once);

            Assert.Empty(existingUser.User.Inventory);
        }

        [Fact]
        public async Task RemoveItemAsync_InsufficientAmount_ThrowsException()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var existingInventory = new List<Inventory>
            {
                new() { ItemId = itemId, Amount = 2, UserId = userId }
            };
            var appUser = new User { Id = userId, Inventory = existingInventory };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _inventoryService.RemoveItemAsync(user, itemId, 3));
        }

        [Fact]
        public async Task HasItemAsync_ItemExists_ReturnsTrue()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var existingInventory = new List<Inventory>
            {
                new() { ItemId = itemId, Amount = 2, UserId = userId }
            };
            var appUser = new User { Id = userId, Inventory = existingInventory };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            var result = await _inventoryService.HasItemAsync(user, itemId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasItemAsync_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            const ulong guildId = 123;
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            var user = new GuildUser { GuildId = guildId, Id = userId };

            var appUser = new User { Id = userId, Inventory = new List<Inventory>() };
            var existingUser = new GuildUser { GuildId = guildId, Id = userId, User = appUser };

            SetupMockDbContextForUser(existingUser);

            // Act
            var result = await _inventoryService.HasItemAsync(user, itemId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetItemCountAsync_ItemExists_ReturnsAmount()
        {
            // Arrange
            const ulong userId = 456;
            var itemId = Guid.NewGuid();
            const int expectedAmount = 5;

            var queryable = new List<GuildUser>
            {
                new()
                {
                    Id = userId,
                    User = new User
                    {
                        Id = userId,
                        Inventory = new List<Inventory>
                        {
                            new() { ItemId = itemId, Amount = expectedAmount, UserId = userId }
                        }
                    }
                }
            }.AsQueryable();

            var mockQueryable = queryable.BuildMockDbSet();
            _mockUserDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockQueryable.Object);

            // Act
            var result = await _inventoryService.GetItemCountAsync(userId, itemId);

            // Assert
            Assert.Equal(expectedAmount, result);
        }

        // Helper method to set up the mock DbContext for user-related operations
        private void SetupMockDbContextForUser(GuildUser existingUser)
        {
            var queryable = new List<GuildUser> { existingUser }.AsQueryable();
            var mockQueryable = queryable.BuildMockDbSet();
            _mockUserDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockQueryable.Object);
        }
    }

    // Extension method to help build mock DbSets
    public static class MockExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> queryable) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }
    }
}