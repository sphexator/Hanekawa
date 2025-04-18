using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hanekawa.Entities.Users;

namespace Hanekawa.Application.Services;

public interface IItemService
{
    ValueTask<Item?> GetItemByIdAsync(Guid itemId);
    ValueTask<Item?> GetItemByNameAsync(string itemName);
    ValueTask<IEnumerable<Item>> GetAllItemsAsync();
    ValueTask<IEnumerable<ItemType>> GetAllItemTypesAsync();
    ValueTask UseItemAsync(ulong guildId, ulong userId, Guid itemId);
    ValueTask<Item> CreateItemAsync(string name, string description, Guid typeId, int? price = null);
}
