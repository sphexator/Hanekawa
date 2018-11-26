using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Account.Storage
{
    public class InventoryManager : IHanaService
    {
        public async Task<EmbedBuilder> GetInventory(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var inventory = await db.Inventories.Where(x => x.GuildId == user.GuildId && x.UserId == user.Id)
                    .ToListAsync();
                var globalInventory = await db.InventoryGlobals.Where(x => x.UserId == user.Id).ToListAsync();
                if ((inventory == null || inventory.Count == 0) && (globalInventory == null || inventory.Count == 0))
                    return new EmbedBuilder().Reply("Your inventory is empty.");
                string inventoryValue = null;

                if (inventory != null || inventory.Count > 0)
                    foreach (var x in inventory)
                        inventoryValue += $"Name: {(await db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n";

                if (globalInventory != null || globalInventory.Count > 0)
                    foreach (var x in globalInventory)
                        inventoryValue += $"Name: {(await db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n";

                if (inventoryValue == null) return new EmbedBuilder().Reply("Your inventory is empty.");
                return new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = inventoryValue,
                    Author = new EmbedAuthorBuilder {IconUrl = user.GetAvatar(), Name = user.Username}
                };
            }
        }
    }
}