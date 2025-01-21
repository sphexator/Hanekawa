using Hanekawa.Application.Extensions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbContext _dbContext;
    private readonly ICacheContext _cache;

    public InventoryService(IDbContext dbContext, ICacheContext cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<GuildUser?> GetInventoryAsync(ulong userId)
    {
        return await _dbContext.Users.Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .ThenInclude(e => e.Item)
            .ThenInclude(e => e.Type)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public ValueTask UpdateInventoryAsync(GuildUser user)
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
            .ThenInclude(e => e.Item)
            .ThenInclude(e => e.Type)
            .FirstOrDefaultAsync(x => x.Id == userId);
        return user?.User.Inventory.FirstOrDefault(x => x.ItemId == itemId)?.Amount ?? 0;
    }

    private async ValueTask<GuildUser> GetInventoryAsync(GuildUser user)
    {
        return await await _cache.GetOrCreateAsync($"inventory_{user.Id}", async () =>
        {
            var userEntity = await _dbContext.Users.Include(e => e.User)
                .ThenInclude(e => e.Inventory)
                .ThenInclude(e => e.Item)
                .ThenInclude(e => e.Type)
                .FirstOrDefaultAsync();
            if (userEntity is null)
            {
                userEntity = await _dbContext.GetOrCreateUserAsync(user.GuildId, user.Id);
            }
            return userEntity;
        });
    }
}

public interface IInventoryService
{
    ValueTask<GuildUser?> GetInventoryAsync(ulong userId);
    ValueTask UpdateInventoryAsync(GuildUser user);
    ValueTask AddItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask RemoveItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId);
    ValueTask<int> GetItemCountAsync(ulong userId, Guid itemId);
}