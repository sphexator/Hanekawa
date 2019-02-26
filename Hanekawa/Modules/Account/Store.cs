using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Stores;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Currency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Extensions;

namespace Hanekawa.Modules.Account
{
    public class Store : InteractiveBase
    {
        private readonly CurrencyService _currencyService;
        public Store(CurrencyService currencyService) => _currencyService = currencyService;

        [Name("Inventory")]
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [Summary("Inventory of user")]
        [Remarks("h.inv")]
        [Ratelimit(1, 2, Measure.Seconds)]
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
                foreach (var x in inventory)
                {
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

                await PagedReplyAsync(result.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Inventory for {Context.User}"));
            }
        }

        [Name("Equip")]
        [Command("equip", RunMode = RunMode.Async)]
        [Alias("use")]
        [Summary("Equips a role you have in your inventory")]
        [Remarks("h.use red")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task EquipRoleAsync([Remainder] IRole role)
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

                var guser = Context.User as SocketGuildUser;
                if (guser.Roles.Contains(role))
                {
                    await Context.ReplyAsync("You already have this role added");
                    return;
                }
                await guser.TryAddRoleAsync(role);
                await Context.ReplyAsync($"{Context.User.Mention} equipped {role.Name}");
            }
        }

        [Name("Unequip")]
        [Command("unequip", RunMode = RunMode.Async)]
        [Alias("unuse")]
        [Summary("Equips a role you have in your inventory")]
        [Remarks("h.use red")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task UnequipRoleAsync([Remainder] IRole role)
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

                var guser = Context.User as SocketGuildUser;
                if (!guser.Roles.Contains(role))
                {
                    await Context.ReplyAsync("You don't have this role added");
                    return;
                }
                await guser.TryAddRoleAsync(role);
                await Context.ReplyAsync($"{Context.User.Mention} unequipped {role.Name}"); ;
            }
        }

        [Name("Store")]
        [Command("store", RunMode = RunMode.Async)]
        [Alias("shop")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Displays the server store")]
        [Remarks("h.shop")]
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
                var regular = store.Where(x => x.SpecialCredit).ToList();
                var special = store.Where(x => !x.SpecialCredit).ToList();
                var result = regular.OrderBy(x => x.Price).Select(x => $"{Context.Guild.GetRole(x.RoleId).Name} - {_currencyService.ToCurrency(cfg, x.Price)}").ToList();
                result.AddRange(special.OrderBy(x => x.Price).Select(x => $"{Context.Guild.GetRole(x.RoleId).Name} - {_currencyService.ToCurrency(cfg, x.Price, true)}"));

                await PagedReplyAsync(result.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Store for {Context.Guild.Name}"));
            }
        }

        [Name("Buy")]
        [Command("buy", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Purchase an item from the store")]
        [Remarks("h.buy red")]
        [Priority(1)]
        [RequiredChannel]
        public async Task BuyAsync([Remainder] IRole role)
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
                if ((serverData.SpecialCredit && userData.CreditSpecial < serverData.Price)
                    || !serverData.SpecialCredit && userData.Credit < serverData.Price)
                {
                    await Context.ReplyAsync(
                        $"You do not have enough special credit for that item. {serverData.Price}",
                        Color.Red.RawValue);
                    return;
                }

                var invItem = await db.Inventories.FirstOrDefaultAsync(x =>
                     x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id && x.ItemId == item.Id);
                if (invItem != null)
                {
                    await Context.ReplyAsync($"You've already purchased {Context.Guild.GetRole(item.Role).Name}.", Color.Red.RawValue);
                    return;
                }
                if (serverData.SpecialCredit) userData.CreditSpecial = userData.CreditSpecial - serverData.Price;
                else userData.Credit = userData.Credit - serverData.Price;

                await db.Inventories.AddAsync(new Inventory
                {
                    Amount = 1,
                    GuildId = Context.Guild.Id,
                    UserId = Context.User.Id,
                    ItemId = item.Id
                });
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Purchased {Context.Guild.GetRole(item.Role)}");
                await (Context.User as SocketGuildUser).TryAddRoleAsync(role);
            }
        }

        [Name("Store add")]
        [Command("store add", RunMode = RunMode.Async)]
        [Alias("sa")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Priority(1)]
        [Summary("Adds an item to the store with regular credit")]
        [Remarks("h.sa 500 red")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddStoreItemAsync(int price, [Remainder] IRole role)
        {
            using (var db = new DbService())
            {
                var date = DateTime.UtcNow;
                var item = new Item
                {
                    GuildId = Context.Guild.Id,
                    Role = role.Id,
                    DateAdded = date
                };
                var storeItem = new ServerStore
                {
                    GuildId = Context.Guild.Id,
                    Price = price,
                    SpecialCredit = false,
                    RoleId = role.Id
                };
                await db.Items.AddAsync(item);
                await db.ServerStores.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Added {role.Name} to the shop for {price}",
                    Color.Green.RawValue);
            }
        }

        [Name("Store add special")]
        [Command("store add special", RunMode = RunMode.Async)]
        [Alias("sas")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Priority(1)]
        [Summary("Adds an item to the store with special credit")]
        [Remarks("h.sas 500 red")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddSpecialStoreItemAsync(int price, [Remainder] IRole role)
        {
            using (var db = new DbService())
            {
                var date = DateTime.UtcNow;
                var item = new Item
                {
                    GuildId = Context.Guild.Id,
                    Role = role.Id,
                    DateAdded = date
                };
                var storeItem = new ServerStore
                {
                    GuildId = Context.Guild.Id,
                    Price = price,
                    SpecialCredit = true,
                    RoleId = role.Id
                };
                await db.Items.AddAsync(item);
                await db.ServerStores.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Added {role.Name} to the shop for {price}",
                    Color.Green.RawValue);
            }
        }

        [Name("Store remove")]
        [Command("store remove", RunMode = RunMode.Async)]
        [Alias("sr")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Removes a role from the store")]
        [Remarks("h.sr red")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveStoreItemAsync([Remainder] IRole role)
        {
            using (var db = new DbService())
            {
                var itemCheck =
                    await db.Items.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Role == role.Id);
                if (itemCheck == null)
                {
                    await Context.ReplyAsync($"{role.Name} is not a part of the store");
                    return;
                }
                var serverItem = await db.ServerStores.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.RoleId == role.Id);

                db.ServerStores.Remove(serverItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {role.Name} from the server store");
            }
        }
    }
}