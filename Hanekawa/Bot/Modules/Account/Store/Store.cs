using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Store
{
    [Name("Store")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public partial class Store : HanekawaModule
    {
        private readonly CurrencyService _currency;
        public Store(CurrencyService currency) => _currency = currency;

        [Name("Inventory")]
        [Command("inventory", "inv")]
        [Description("Inventory of user")]
        [RequiredChannel]
        public async Task InventoryAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories
                .Where(x => x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue).ToListAsync();
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
                if (item == null || !item.Role.HasValue) continue;
                var role = Context.Guild.GetRole(item.Role.Value);
                if (role == null) continue;
                result.Add($"{role.Name} - Amount: {x.Amount}");
            }

            if (result.Count == 0)
            {
                await Context.ReplyAsync("Your inventory is empty");
                return;
            }

            await Context.PaginatedReply(result, Context.Member, $"Inventory for {Context.User}");
        }

        [Name("Equip")]
        [Command("equip", "use")]
        [Description("Equips a role you have in your inventory")]
        [RequiredChannel]
        public async Task EquipRoleAsync([Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories
                .Where(x => x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue).ToListAsync();
            var item = await db.Items.FirstOrDefaultAsync(z => z.GuildId == Context.Guild.Id.RawValue && z.Role == role.Id.RawValue);
            if (inventory.FirstOrDefault(x => x.ItemId == item.Id) == null)
            {
                await Context.ReplyAsync("You do not own this item");
                return;
            }

            if (Context.Member.Roles.Values.Contains(role))
            {
                await Context.ReplyAsync("You already have this role added");
                return;
            }

            await Context.Member.TryAddRoleAsync(role);
            await Context.ReplyAsync($"{Context.User.Mention} equipped {role.Name}");
        }

        [Name("Unequip")]
        [Command("unequip", "unuse")]
        [Description("Equips a role you have in your inventory")]
        [RequiredChannel]
        public async Task UnequipRoleAsync([Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories
                .Where(x => x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue).ToListAsync();
            var item = await db.Items.FirstOrDefaultAsync(z => z.GuildId == Context.Guild.Id.RawValue && z.Role == role.Id.RawValue);
            if (inventory.FirstOrDefault(x => x.ItemId == item.Id) == null)
            {
                await Context.ReplyAsync("You can't remove a role you do not own or have.");
                return;
            }

            if (!Context.Member.Roles.Values.Contains(role))
            {
                await Context.ReplyAsync("You don't have this role added");
                return;
            }

            await Context.Member.TryRemoveRoleAsync(role);
            await Context.ReplyAsync($"{Context.User.Mention} unequipped {role.Name}");
        }

        [Name("Store")]
        [Command("store", "shop")]
        [Description("Displays the server store")]
        [RequiredChannel]
        public async Task ServerShopAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            var store = await db.ServerStores.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            if (store.Count == 0)
            {
                await Context.ReplyAsync("Store is empty");
                return;
            }

            var result = new List<string>();
            for (var i = 0; i < store.Count; i++)
            {
                var x = store[i];
                result.Add(
                    $"{Context.Guild.GetRole(x.RoleId).Name ?? "No role found"} - {_currency.ToCurrency(cfg, x.Price, x.SpecialCredit)}");
            }

            await Context.PaginatedReply(result, Context.Guild, $"Store for {Context.Guild.Name}");
        }

        [Name("Buy")]
        [Command("buy")]
        [Description("Purchase an item from the store")]
        [Priority(1)]
        [RequiredChannel]
        public async Task BuyAsync([Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var serverData =
                await db.ServerStores.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id.RawValue && x.RoleId == role.Id.RawValue);
            if (serverData == null)
            {
                await Context.ReplyAsync("Couldn't find an item with that ID.");
                return;
            }

            var userData = await db.GetOrCreateUserData(Context.Member);
            var item = await db.Items.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id.RawValue && x.Role == role.Id.RawValue);
            var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (serverData.SpecialCredit)
            {
                if (userData.CreditSpecial < serverData.Price)
                {
                    await Context.ReplyAsync(
                        $"You do not have enough {_currency.ToCurrency(creditCfg, serverData.Price, true)} to purchase {role.Name}",
                        Color.Red);
                    return;
                }
            }
            else
            {
                if (userData.Credit < serverData.Price)
                {
                    await Context.ReplyAsync(
                        $"You do not have enough {_currency.ToCurrency(creditCfg, serverData.Price)} to purchase {role.Name}",
                        Color.Red);
                    return;
                }
            }

            var invItem = await db.Inventories.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue && x.ItemId == item.Id);
            if (invItem != null)
            {
                await Context.ReplyAsync($"You've already purchased {Context.Guild.GetRole(item.Role.Value).Name}.",
                    Color.Red);
                return;
            }

            if (serverData.SpecialCredit) userData.CreditSpecial -= serverData.Price;
            else userData.Credit -= serverData.Price;

            await db.Inventories.AddAsync(new Inventory
            {
                Amount = 1,
                GuildId = Context.Guild.Id.RawValue,
                UserId = Context.User.Id.RawValue,
                ItemId = item.Id
            });
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Purchased {Context.Guild.GetRole(item.Role.Value)} for {_currency.ToCurrency(creditCfg, serverData.Price, serverData.SpecialCredit)}",
                Color.Green);
            await Context.Member.TryAddRoleAsync(role);
        }
    }
}