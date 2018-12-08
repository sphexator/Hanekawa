﻿using System.Linq;
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
        private readonly DbService _db;

        public InventoryManager(DbService db)
        {
            _db = db;
        }

        public async Task<EmbedBuilder> GetInventory(IGuildUser user)
        {
            var inventory = await _db.Inventories.Where(x => x.GuildId == user.GuildId && x.UserId == user.Id)
                .ToListAsync();
            var globalInventory = await _db.InventoryGlobals.Where(x => x.UserId == user.Id).ToListAsync();
            if (inventory.Count == 0) return new EmbedBuilder().CreateDefault("Your inventory is empty.");

            var inventoryValue = new StringBuilder();

            if (inventory.Count > 0)
                foreach (var x in inventory)
                    inventoryValue.Append($"Name: {(await _db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n");

            if (globalInventory.Count > 0)
                foreach (var x in globalInventory)
                    inventoryValue.Append($"Name: {(await _db.Items.FindAsync(x.ItemId)).Name} - Amount: {x.Amount}\n");

            if (inventoryValue.Length == 0) return new EmbedBuilder().CreateDefault("Your inventory is empty.");

            return new EmbedBuilder().CreateDefault(inventoryValue.ToString()).WithAuthor(new EmbedAuthorBuilder
                {IconUrl = user.GetAvatar(), Name = user.Username});
        }
    }
}