using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Services;

public class ItemService : IItemService
{
    private readonly IDbContext _dbContext;
    private readonly ICacheContext _cache;

    public ItemService(IDbContext dbContext, ICacheContext cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<Item?> GetItemByIdAsync(Guid itemId)
    {
        return await _cache.GetOrCreateAsync($"item_{itemId}", async () =>
        {
            return await _dbContext.Items
                .Include(e => e.Type)
                .FirstOrDefaultAsync(x => x.Id == itemId);
        });
    }

    public async ValueTask<Item?> GetItemByNameAsync(string itemName)
    {
        // Don't cache this query as it's by name
        return await _dbContext.Items
            .Include(e => e.Type)
            .FirstOrDefaultAsync(x => x.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask<IEnumerable<Item>> GetAllItemsAsync()
    {
        return await _cache.GetOrCreateAsync("all_items", async () =>
        {
            return await _dbContext.Items
                .Include(e => e.Type)
                .ToListAsync();
        });
    }

    public async ValueTask<IEnumerable<ItemType>> GetAllItemTypesAsync()
    {
        return await _cache.GetOrCreateAsync("all_item_types", async () =>
        {
            return await _dbContext.ItemTypes.ToListAsync();
        });
    }

    public async ValueTask UseItemAsync(ulong guildId, ulong userId, Guid itemId)
    {
        var item = await GetItemByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found");

        // Implement item-specific effects here based on item type or properties
        switch (item.Type.Name.ToLowerInvariant())
        {
            case "consumable":
                // Implement consumable effect
                break;
            case "collectible":
                // Collectibles might not have an effect when used
                break;
            case "utility":
                // Implement utility effect
                break;
            default:
                throw new NotSupportedException($"Item type '{item.Type.Name}' has no use implementation");
        }
    }

    public async ValueTask<Item> CreateItemAsync(string name, string description, Guid typeId, int? price = null)
    {
        var itemType = await _dbContext.ItemTypes.FindAsync(typeId);
        if (itemType == null)
            throw new ArgumentException("Item type not found", nameof(typeId));

        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            TypeId = typeId,
            Price = price
        };

        await _dbContext.Items.AddAsync(item);
        await _dbContext.SaveChangesAsync();
        _cache.Remove("all_items");

        return item;
    }
}