using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Entities.Items;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Store")]
    [Description("Commands for server store")]
    [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.SendMessages)]
    public class Store : HanekawaCommandModule
    {
        [Name("Store")]
        [Description("Displays the server store")]
        [Command("store")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> ShopAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            var store = await db.ServerStores.Where(x => x.GuildId == Context.GuildId).ToListAsync();
            if (store.Count == 0)
                return Reply("Store is empty",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));

            var result = new List<string>();
            foreach (var x in store)
            {
                if(Context.Guild.Roles.TryGetValue(x.RoleId, out var role)) result.Add(role.Name);
            }

            if (result.Count == 0)
                return Reply("Store is empty, or couldn't find roles within the store",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));
            return Pages(result.PaginationBuilder(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"Store for {Context.Guild.Name}"));
        }

        [Name("Inventory")]
        [Description("Display your inventory")]
        [Command("inventory", "inv")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> InventoryAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories.FindAsync(Context.Author.Id);
            if (inventory.Items.Count == 0)
                return Reply("Your inventory is empty",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));

            var result = new List<string>();
            foreach (var x in inventory.Items)
            {
                if (x.ItemJson is RoleItem roleItem)
                {
                    if(!Context.Guild.Roles.TryGetValue(roleItem.RoleId, out var role)) continue;
                    result.Add($"{role.Name} [Role]");
                }
                else result.Add($"{x.ItemJson.Name} [{x.GetType()}]");
            }

            return result.Count == 0
                ? Reply("Your inventory is empty",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId))
                : Pages(result.PaginationBuilder(
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                    Context.Author.GetAvatarUrl(), $"Inventory for {Context.Author}"));
        }
        
        [Name("Equip")]
        [Command("equip", "use")]
        [Description("Equips a role you have in your inventory")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> EquipRoleAsync([Remainder] IRole role)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories.FindAsync(Context.Author.Id);
            if (inventory.Items.FirstOrDefault(x => ((RoleItem) x.ItemJson).RoleId == role.Id) == null)
                return Reply($"You currently do not own this role, or it's not available!", HanaBaseColor.Bad());
            if (Context.Author.RoleIds.Contains(role.Id)) return Reply("You already have this role added");
            
            var result = await Context.Author.TryAddRoleAsync(role);
            if(result) return Reply($"{Context.Author.Mention} equipped {role.Name}", Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));
            return Reply(
                $"Couldn't add role {role.Name}. I require Manage Roles permission, or be above the role in the role hierarchy",
                HanaBaseColor.Bad());
        }

        [Name("Unequip")]
        [Command("unequip", "unuse")]
        [Description("Equips a role you have in your inventory")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> UnEquipRoleAsync([Remainder] IRole role)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var inventory = await db.Inventories.FindAsync(Context.Author.Id);
            if (inventory.Items.FirstOrDefault(x => ((RoleItem) x.ItemJson).RoleId == role.Id) == null)
                return Reply("You can't remove a role you do not own or have.", HanaBaseColor.Bad());
            
            if (!Context.Author.RoleIds.Contains(role.Id))
                return Reply("You don't have this role added", HanaBaseColor.Bad());
            
            await Context.Author.TryRemoveRoleAsync(role);
            return Reply($"{Context.Author.Mention} unequipped {role.Name}", HanaBaseColor.Ok());
        }
    }

    [Group("Store")]
    [Name("Store Admin")]
    [Description("Commands for store management")]
    public class StoreAdmin : Store
    {
        
    }
}