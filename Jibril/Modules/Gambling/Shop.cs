using System;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Gambling
{
    public class Shop : ModuleBase<SocketCommandContext>
    {
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [RequiredChannel(339383206669320192)]
        public async Task UserInventory()
        {
            using (var db = new hanekawaContext())
            {
                var inventory = await db.GetOrCreateInventory(Context.User);

                string[] items = { "Repair Kit", "Damage Boost", "Shield", "Custom Role", "Gift" };
                await ReplyAsync($"{Context.User.Username} Inventory:" +
                                 "```" +
                                 $"{items[0].PadRight(22)}   {inventory.RepairKit}\n" +
                                 $"{items[1].PadRight(22)}   {inventory.DamageBoost}\n" +
                                 $"{items[2].PadRight(22)}   {inventory.Shield}\n" +
                                 $"{items[3].PadRight(22)}   {inventory.Customrole}\n" +
                                 $"{items[4].PadRight(22)}   {inventory.Gift}\n" +
                                 "```");
            }
        }

        [Command("shop", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task Shoplist()
        {
            using (var db = new hanekawaContext())
            {
                var shoplist = db.Shop.ToList();

                var embed = EmbedGenerator.DefaultEmbed($"To buy, use !buy <number>", Colours.DefaultColour);
                var numba = 1;
                foreach (var x in shoplist)
                {
                    embed.AddField(y =>
                    {
                        y.Name = $"{numba}: {x.Item}";
                        y.Value = $"${x.Price}";
                        y.IsInline = true;
                    });
                    numba++;
                }
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("eventshop", RunMode = RunMode.Async)]
        [Alias("eshop")]
        [RequiredChannel(339383206669320192)]
        public async Task EventShop()
        {
            using (var db = new hanekawaContext())
            {
                var shoplist = db.Eventshop.ToList();

                var embed = EmbedGenerator.DefaultEmbed($"To buy, use !ebuy <number>", Colours.DefaultColour);
                var numba = 1;
                foreach (var x in shoplist)
                {
                    embed.AddField(y =>
                    {
                        y.Name = $"{numba}: {x.Item} Stock: {x.Stock}";
                        y.Value = $"{x.Price}";
                        y.IsInline = true;
                    });
                    numba++;
                }
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("buy", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task Buy(int itemId, [Remainder] int amount = 1)
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var inventory = await db.GetOrCreateInventory(user);
                var item = await db.Shop.FindAsync(itemId);
                if (item == null) return;
                var cost = Convert.ToUInt32(item.Price * amount);
                if (userdata.Tokens >= cost)
                {
                    switch (item.Item)
                    {
                        case "RepairKit":
                            inventory.RepairKit = inventory.RepairKit + amount;
                            break;
                        case "DamageBoost":
                            inventory.DamageBoost = inventory.DamageBoost + amount;
                            break;
                        case "Shield":
                            inventory.Shield = inventory.Shield + amount;
                            break;
                        case "Gift":
                            inventory.Gift = inventory.Gift + amount;
                            break;
                    }

                    userdata.Tokens = userdata.Tokens - cost;
                    await db.SaveChangesAsync();

                    var embed = EmbedGenerator.DefaultEmbed(
                        $"{user.Username} bought {item.Item} for ${item.Price}", Colours.OkColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You don't have enough money for that.",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("eventbuy", RunMode = RunMode.Async)]
        [Alias("ebuy")]
        [RequiredChannel(339383206669320192)]
        public async Task EventBuy(int itemId)
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var inventory = await db.GetOrCreateInventory(user);
                var item = await db.Eventshop.FindAsync(itemId);
                if (item == null || item.Stock == 0) return;
                if (userdata.EventTokens >= item.Price)
                {
                    userdata.EventTokens = userdata.EventTokens - Convert.ToUInt32(item.Price);
                    inventory.Customrole = inventory.Customrole + 1;
                    item.Stock = item.Stock - 1;
                    await db.SaveChangesAsync();
                    var embed = EmbedGenerator.DefaultEmbed(
                        $"{user.Username} bought {item.Item} for {item.Price} tokens",
                        Colours.OkColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
                else if (userdata.EventTokens < item.Price)
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You don't have enough for that.",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("createrole", RunMode = RunMode.Async)]
        [Alias("cr", "crole")]
        [RequiredChannel(339383206669320192)]
        public async Task CreateRole([Remainder] string name)
        {
            var user = Context.User as IGuildUser;
            using (var db = new hanekawaContext())
            {
                var inventory = await db.GetOrCreateInventory(user);
                var userdata = await db.GetOrCreateUserData(user);
                if (inventory.Customrole > 0 && userdata.Hasrole != "yes")
                {
                    inventory.Customrole = inventory.Customrole - 1;
                    userdata.Hasrole = "yes";
                    await db.SaveChangesAsync();

                    var role = await Context.Guild.CreateRoleAsync($"{name}", GuildPermissions.None);
                    await user.AddRoleAsync(role);
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} created role {name}", Colours.OkColour);
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed(
                        $"{user.Username}: You need to buy a role or you've already bought a role", Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                }
            }
        }
    }
}