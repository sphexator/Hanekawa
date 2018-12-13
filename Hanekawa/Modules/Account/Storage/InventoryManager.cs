using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
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
                if (inventory.Count == 0) return new EmbedBuilder().CreateDefault("Your inventory is empty.", user.GuildId);

                var inventoryValue = new StringBuilder();

                if (inventory.Count > 0)
                    foreach (var x in inventory)
                        inventoryValue.Append($"Name: {(await db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n");

                if (globalInventory.Count > 0)
                    foreach (var x in globalInventory)
                        inventoryValue.Append($"Name: {(await db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n");

                if (inventoryValue.Length == 0) return new EmbedBuilder().CreateDefault("Your inventory is empty.", user.GuildId);

                return new EmbedBuilder().CreateDefault(inventoryValue.ToString(), user.GuildId).WithAuthor(new EmbedAuthorBuilder
                    { IconUrl = user.GetAvatar(), Name = user.Username });
            }
        }
    }
}