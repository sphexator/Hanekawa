﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Store
{
    [Name("Store")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Store : InteractiveBase
    {
        private readonly CurrencyService _currency;
        public Store(CurrencyService currency) => _currency = currency;

        [Name("Inventory")]
        [Command("inventory", "inv")]
        [Description("Inventory of user")]
        [Remarks("inv")]
        [RequiredChannel]
        public async Task InventoryAsync()
        {
            using (var db = new DbService())
            {
                var inventory = await db.Inventories
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                if (inventory.Count == 0)
                {
                    await Context.ReplyAsync("Your inventory is empty");
                    return;
                }

                var result = new List<string>();
                for (var i = 0; i < inventory.Count; i++)
                {
                    var x = inventory[i];
                    var item = await db.Items.FirstOrDefaultAsync(z => z.Id == x.ItemId);
                    if (item == null) continue;
                    var role = Context.Guild.GetRole(item.Role);
                    if (role == null) continue;
                    result.Add($"{role.Name} - Amount: {x.Amount}");
                }

                if (result.Count == 0)
                {
                    await Context.ReplyAsync("Your inventory is empty");
                    return;
                }

                await PagedReplyAsync(result.PaginateBuilder(Context.Guild, $"Inventory for {Context.User}", null));
            }
        }

        [Name("Equip")]
        [Command("equip", "use")]
        [Description("Equips a role you have in your inventory")]
        [Remarks("use red")]
        [RequiredChannel]
        public async Task EquipRoleAsync([Remainder] SocketRole role)
        {
            using (var db = new DbService())
            {
                var inventory = await db.Inventories
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                var item = await db.Items.FirstOrDefaultAsync(z => z.GuildId == Context.Guild.Id && z.Role == role.Id);
                if (inventory.FirstOrDefault(x => x.ItemId == item.Id) == null)
                {
                    await Context.ReplyAsync("You do not own this item");
                    return;
                }

                if (Context.User.Roles.Contains(role))
                {
                    await Context.ReplyAsync("You already have this role added");
                    return;
                }
                await Context.User.TryAddRoleAsync(role);
                await Context.ReplyAsync($"{Context.User.Mention} equipped {role.Name}");
            }
        }

        [Name("Unequip")]
        [Command("unequip", "unuse")]
        [Description("Equips a role you have in your inventory")]
        [Remarks("unuse red")]
        [RequiredChannel]
        public async Task UnequipRoleAsync([Remainder] SocketRole role)
        {
            using (var db = new DbService())
            {
                var inventory = await db.Inventories
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                var item = await db.Items.FirstOrDefaultAsync(z => z.GuildId == Context.Guild.Id && z.Role == role.Id);
                if (inventory.FirstOrDefault(x => x.ItemId == item.Id) == null)
                {
                    await Context.ReplyAsync("You can't remove a role you do not own or have.");
                    return;
                }
                if (!Context.User.Roles.Contains(role))
                {
                    await Context.ReplyAsync("You don't have this role added");
                    return;
                }

                await Context.User.TryRemoveRoleAsync(role);
                await Context.ReplyAsync($"{Context.User.Mention} unequipped {role.Name}"); ;
            }
        }

        [Name("Store")]
        [Command("store", "shop")]
        [Description("Displays the server store")]
        [Remarks("shop")]
        [RequiredChannel]
        public async Task ServerShopAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                var store = await db.ServerStores.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (store.Count == 0)
                {
                    await Context.ReplyAsync("Store is empty");
                    return;
                }

                var result = new List<string>();
                for (var i = 0; i < store.Count; i++)
                {
                    var x = store[i];
                    result.Add($"{Context.Guild.GetRole(x.RoleId).Name ?? "No role found"} - {_currency.ToCurrency(cfg, x.Price, x.SpecialCredit)}");
                }
                await PagedReplyAsync(result.PaginateBuilder(Context.Guild, $"Store for {Context.Guild.Name}", null));
            }
        }

        [Name("Buy")]
        [Command("buy")]
        [Description("Purchase an item from the store")]
        [Remarks("buy red")]
        [Priority(1)]
        [RequiredChannel]
        public async Task BuyAsync([Remainder] SocketRole role)
        {
            using (var db = new DbService())
            {
                var serverData =
                    await db.ServerStores.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if (serverData == null)
                {
                    await Context.ReplyAsync("Couldn't find an item with that ID.");
                    return;
                }

                var userData = await db.GetOrCreateUserData(Context.User as IGuildUser);
                var item = await db.Items.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Role == role.Id);
                var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (serverData.SpecialCredit)
                {
                    if (userData.CreditSpecial < serverData.Price)
                    {
                        await Context.ReplyAsync(
                            $"You do not have enough {_currency.ToCurrency(creditCfg, serverData.Price, true)} to purchase {role.Name}",
                            Color.Red.RawValue);
                        return;
                    }
                }
                else
                {
                    if (userData.Credit < serverData.Price)
                    {
                        await Context.ReplyAsync(
                            $"You do not have enough {_currency.ToCurrency(creditCfg, serverData.Price)} to purchase {role.Name}",
                            Color.Red.RawValue);
                        return;
                    }
                }

                var invItem = await db.Inventories.FirstOrDefaultAsync(x =>
                     x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id && x.ItemId == item.Id);
                if (invItem != null)
                {
                    await Context.ReplyAsync($"You've already purchased {Context.Guild.GetRole(item.Role).Name}.", Color.Red.RawValue);
                    return;
                }
                if (serverData.SpecialCredit) userData.CreditSpecial -= serverData.Price;
                else userData.Credit -= serverData.Price;

                await db.Inventories.AddAsync(new Inventory
                {
                    Amount = 1,
                    GuildId = Context.Guild.Id,
                    UserId = Context.User.Id,
                    ItemId = item.Id
                });
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Purchased {Context.Guild.GetRole(item.Role)} for {_currency.ToCurrency(creditCfg, serverData.Price, serverData.SpecialCredit)}",
                    Color.Green.RawValue);
                await Context.User.TryAddRoleAsync(role);
            }
        }
    }
}