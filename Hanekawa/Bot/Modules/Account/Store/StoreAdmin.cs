using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Stores;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Store
{
    public partial class Store
    {
        [Name("Store add")]
        [Command("sa")]
        [Description("Adds an item to the store with regular credit")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddStoreItemAsync(int price, [Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var date = DateTime.UtcNow;
            var item = new Item
            {
                GuildId = Context.Guild.Id.RawValue,
                Role = role.Id.RawValue,
                DateAdded = date
            };
            var storeItem = new ServerStore
            {
                GuildId = Context.Guild.Id.RawValue,
                Price = price,
                SpecialCredit = false,
                RoleId = role.Id.RawValue
            };
            var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            await db.Items.AddAsync(item);
            await db.ServerStores.AddAsync(storeItem);
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Added {role.Name} to the shop for {_currency.ToCurrency(creditCfg, price)}",
                Color.Green);
        }

        [Name("Store add special")]
        [Command("sas")]
        [Description("Adds an item to the store with special credit")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddSpecialStoreItemAsync(int price, [Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var date = DateTime.UtcNow;
            var item = new Item
            {
                GuildId = Context.Guild.Id.RawValue,
                Role = role.Id.RawValue,
                DateAdded = date
            };
            var storeItem = new ServerStore
            {
                GuildId = Context.Guild.Id.RawValue,
                Price = price,
                SpecialCredit = true,
                RoleId = role.Id.RawValue
            };
            var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            await db.Items.AddAsync(item);
            await db.ServerStores.AddAsync(storeItem);
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Added {role.Name} to the shop for {_currency.ToCurrency(creditCfg, price, true)}",
                Color.Green);
        }

        [Name("Store remove")]
        [Command("sr")]
        [Description("Removes a role from the store")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveStoreItemAsync([Remainder] CachedRole role)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var itemCheck =
                await db.Items.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id.RawValue && x.Role == role.Id.RawValue);
            if (itemCheck == null)
            {
                await Context.ReplyAsync($"{role.Name} is not a part of the store");
                return;
            }

            var serverItem = await db.ServerStores.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.RoleId == role.Id.RawValue);

            db.ServerStores.Remove(serverItem);
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Removed {role.Name} from the server store");
        }
    }
}