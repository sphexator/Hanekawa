using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Services;

public interface IInventoryService
{
    ValueTask<GuildUser?> GetInventoryAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default);
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

    public async ValueTask<GuildUser?> GetInventoryAsync(ulong guildId, ulong userId,
        CancellationToken cancellationToken = default)
    {
        return await GetOrCreateInventoryAsync(guildId, userId, cancellationToken);
    }

    public ValueTask UpdateInventoryAsync(GuildUser user, Inventory inventory)
    {
        throw new NotImplementedException();
    }

    public ValueTask AddItemAsync(GuildUser user, Guid itemId, int amount)
    {
        throw new NotImplementedException();
    }

    public ValueTask RemoveItemAsync(GuildUser user, Guid itemId, int amount)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId)
    {
        throw new NotImplementedException();
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