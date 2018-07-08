using System.Collections.Generic;
using System.Linq;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;
using System.Threading.Tasks;
using Discord;
using Jibril.Services.Entities.Tables;

namespace Jibril.Modules.Account.Shop
{
    public class Shop : InteractiveBase
    {
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        public async Task InventoryAsync()
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User);
                var inv = $"{(Context.User as SocketGuildUser).GetName()} Inventory:\n";
                if (userdata.Inventory.Count != 0)
                {
                    inv += "```\n";
                    foreach (var x in userdata.Inventory)
                    {
                        var data = $"{x.Name.PadRight(22)} {x.Amount}\n";
                        inv += data;
                    }
                    inv += "```";
                }
                await ReplyAsync(inv);
            }
        }

        [Command("shop", RunMode = RunMode.Async)]
        public async Task ShopAsync()
        {
            using (var db = new DbService())
            {
                var author = new EmbedAuthorBuilder
                {
                    Name = "Shop"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.DarkPurple
                };
                foreach (var x in db.Shops)
                {
                    if (x.RoleId.HasValue)
                    {
                        var field = new EmbedFieldBuilder
                        {
                            Name = $"Role: {x.Item}",
                            Value = $"{x.Price}",
                            IsInline = false
                        };
                        embed.AddField(field);
                    }
                    else
                    {
                        var field = new EmbedFieldBuilder
                        {
                            Name = $"{x.Item}",
                            Value = $"{x.Price}",
                            IsInline = false
                        };
                        embed.AddField(field);
                    }
                }

                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("buy", RunMode = RunMode.Async)]
        public async Task BuyAsync(uint itemId)
        {
            using (var db = new DbService())
            {
                var getItem = await db.Shops.FindAsync(itemId);
                if (getItem == null) return;
                var userdata = await db.GetOrCreateUserData(Context.User);
                if (userdata.Credit < getItem.Price)
                {
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} don't have enough credit to buy that item!", Color.Red.RawValue).Build());
                    return;
                }

                userdata.Credit = userdata.Credit - getItem.Price;
                var invCheck = userdata.Inventory.FirstOrDefault(x => x.Name == getItem.Item);
                if (invCheck == null)
                {
                    userdata.Inventory = userdata.Inventory;
                }
            }
        }
    }
}
