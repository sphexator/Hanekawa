using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Services;

public interface IInventoryService
{
    ValueTask<GuildUser> GetInventoryAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default);
    ValueTask UpdateInventoryAsync(GuildUser user, List<Inventory> inventory);
    ValueTask UpdateInventoryAsync(GuildUser user, Inventory inventory);
    ValueTask AddItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask RemoveItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId);
    ValueTask<int> GetItemCountAsync(ulong userId, Guid itemId);
}

public class InventoryService : IInventoryService
{
    private readonly IDbContext _dbContext;
    private readonly ICacheContext _cache;

    public InventoryService(IDbContext dbContext, ICacheContext cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public ValueTask<GuildUser> GetInventoryAsync(ulong guildId, ulong userId,
        CancellationToken cancellationToken = default)
    {
        return GetOrCreateInventoryAsync(guildId, userId, cancellationToken);
    }

    public async ValueTask UpdateInventoryAsync(GuildUser user, List<Inventory> inventory)
    {
        var existingUser = await _dbContext.Users
            .Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .FirstOrDefaultAsync(x => x.GuildId == user.GuildId && x.Id == user.Id);

        if (existingUser == null) return;

        // Replace inventory items
        existingUser.User.Inventory = inventory;

        await _dbContext.SaveChangesAsync();
        _cache.Remove($"inventory_{user.Id}");
    }
    public async ValueTask UpdateInventoryAsync(GuildUser user, Inventory inventory)
    {
        var existingUser = await _dbContext.Users
            .Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .FirstOrDefaultAsync(x => x.GuildId == user.GuildId && x.Id == user.Id);

        if (existingUser == null) return;

        // Replace inventory items
        var item = existingUser.User.Inventory.FirstOrDefault(e => e.ItemId == inventory.ItemId);
        if (item is not null)
        {
            item.Amount =+ inventory.Amount;
        }
        else
        {
            existingUser.User.Inventory.Add(inventory);
        }

        await _dbContext.SaveChangesAsync();
        _cache.Remove($"inventory_{user.Id}");
    }

    public async ValueTask AddItemAsync(GuildUser user, Guid itemId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        var existingUser = await _dbContext.Users
            .Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .FirstOrDefaultAsync(x => x.GuildId == user.GuildId && x.Id == user.Id);

        if (existingUser == null)
            return;

        var item = existingUser.User.Inventory.FirstOrDefault(x => x.ItemId == itemId);
        if (item != null)
        {
            item.Amount += amount;
        }
        else
        {
            existingUser.User.Inventory.Add(new Inventory
            {
                ItemId = itemId,
                Amount = amount,
                UserId = user.Id
            });
        }

        await _dbContext.SaveChangesAsync();
        _cache.Remove($"inventory_{user.Id}");
    }

    public async ValueTask RemoveItemAsync(GuildUser user, Guid itemId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        var existingUser = await _dbContext.Users
            .Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .FirstOrDefaultAsync(x => x.GuildId == user.GuildId && x.Id == user.Id);

        if (existingUser == null)
            return;

        var item = existingUser.User.Inventory.FirstOrDefault(x => x.ItemId == itemId);
        if (item == null)
            throw new InvalidOperationException("Item not found in inventory");

        if (item.Amount < amount)
            throw new InvalidOperationException("Not enough items in inventory");

        item.Amount -= amount;
        if (item.Amount == 0)
        {
            existingUser.User.Inventory.Remove(item);
        }

        await _dbContext.SaveChangesAsync();
        _cache.Remove($"inventory_{user.Id}");
    }

    public async ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId)
    {
        var existingUser = await _dbContext.Users
            .Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .FirstOrDefaultAsync(x => x.GuildId == user.GuildId && x.Id == user.Id);

        if (existingUser == null)
            return false;

        return existingUser.User.Inventory.Any(x => x.ItemId == itemId);
    }

    public async ValueTask<int> GetItemCountAsync(ulong userId, Guid itemId)
    {
        var user = await _dbContext.Users.Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .Select(e => new
            {
                e.Id,
                e.User.Inventory.FirstOrDefault(x => x.ItemId == itemId)!.Amount
            })
            .FirstOrDefaultAsync(x => x.Id == userId);
        return user?.Amount ?? 0;
    }

    private async ValueTask<GuildUser> GetOrCreateInventoryAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync($"inventory_{userId}", async () =>
        {
            var userEntity = await _dbContext.Users.Include(e => e.User)
                .ThenInclude(e => e.Inventory)
                .ThenInclude(e => e.Item)
                .ThenInclude(e => e.Type)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == userId, cancellationToken: cancellationToken);
            if (userEntity is null)
            {
                userEntity = await _dbContext.GetOrCreateUserAsync(guildId, userId, cancellationToken: cancellationToken);
            }
            return userEntity;
        });
    }
}