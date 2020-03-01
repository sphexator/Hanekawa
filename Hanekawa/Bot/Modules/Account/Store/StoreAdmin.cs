using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Stores;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Store
{
    public partial class Store
    {
        [Name("Store add")]
        [Command("sa")]
        [Priority(1)]
        [Description("Adds an item to the store with regular credit")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddStoreItemAsync(int price, [Remainder] SocketRole role)
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
                var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                await db.Items.AddAsync(item);
                await db.ServerStores.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Added {role.Name} to the shop for {_currency.ToCurrency(creditCfg, price)}",
                    Color.Green);
            }
        }

        [Name("Store add special")]
        [Command("sas")]
        [Priority(1)]
        [Description("Adds an item to the store with special credit")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddSpecialStoreItemAsync(int price, [Remainder] SocketRole role)
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
                var creditCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                await db.Items.AddAsync(item);
                await db.ServerStores.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Added {role.Name} to the shop for {_currency.ToCurrency(creditCfg, price, true)}",
                    Color.Green);
            }
        }

        [Name("Store remove")]
        [Command("sr")]
        [Description("Removes a role from the store")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveStoreItemAsync([Remainder] SocketRole role)
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