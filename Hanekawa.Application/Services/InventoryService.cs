using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbContext _dbContext;

    public InventoryService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<GuildUser?> GetInventoryAsync(ulong userId)
    {
        return await _dbContext.Users.Include(e => e.User)
            .ThenInclude(e => e.Inventory)
            .ThenInclude(e => e.Item)
            .ThenInclude(e => e.Type)
            .FirstOrDefaultAsync(x => x != null && x.Id == userId);
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
    public ValueTask<int> GetItemCountAsync(ulong userId, Guid itemId)
    {
        throw new NotImplementedException();
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