using Hanekawa.Entities.Users;

namespace Hanekawa.Application.Services;

public class InventoryService
{

}

public interface IInventoryService
{
    ValueTask<GuildUser> GetInventoryAsync(ulong userId);
    ValueTask UpdateInventoryAsync(GuildUser user);
    ValueTask AddItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask RemoveItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId, int amount);
    ValueTask<bool> HasItemAsync(GuildUser user, Guid itemId);
    ValueTask<int> GetItemCountAsync(ulong userId, Guid itemId);
}