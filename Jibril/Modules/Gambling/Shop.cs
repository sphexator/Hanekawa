using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Gambling.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Gambling
{
    public class Shop : ModuleBase<SocketCommandContext>
    {
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [RequiredChannel(339383206669320192)]
        public async Task UserInventory()
        {
            var user = Context.User;
            var inventoryCheck = GambleDB.Inventory(user).FirstOrDefault();
            if (inventoryCheck == null)
                GambleDB.CreateInventory(user);
            var inventory = GambleDB.Inventory(user).FirstOrDefault();

            string[] items = {"Repair Kit", "Damage Boost", "Shield", "Custom Role"};
            var embed = new EmbedBuilder();
            var author = new EmbedAuthorBuilder();

            author.WithIconUrl(user.GetAvatarUrl());
            author.WithName(user.Username);
            embed.WithAuthor(author);
            embed.WithColor(new Color(Colours.DefaultColour));
            embed.Description = $"" +
                                $"{items[0].PadRight(22)}   {inventory.Repairkit}\n" +
                                $"{items[1].PadRight(15)}   {inventory.Dmgboost}\n" +
                                $"{items[2].PadRight(25)}   {inventory.Shield}\n" +
                                $"{items[3].PadRight(17)}   {inventory.CustomRole}";
            await ReplyAsync($"", false, embed.Build());
        }

        [Command("shop", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task Shoplist()
        {
            var shoplist = GambleDB.Shoplist().ToList();

            var embed = EmbedGenerator.DefaultEmbed($"To buy, use !buy <number>", Colours.DefaultColour);
            for (var i = 0; i < 3; i++)
            {
                var c = shoplist[i];
                embed.AddField(y =>
                {
                    y.Name = $"{i}: {c.Item}";
                    y.Value = $"${c.Price}";
                    y.IsInline = true;
                });
            }

            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("eventshop", RunMode = RunMode.Async)]
        [Alias("eshop")]
        [RequiredChannel(339383206669320192)]
        public async Task EventShop()
        {
            var shoplist = GambleDB.EventShopList().ToList();

            var embed = EmbedGenerator.DefaultEmbed($"To buy, use !ebuy <number>", Colours.DefaultColour);
            for (var i = 0; i < 1; i++)
            {
                var c = shoplist[i];
                embed.AddField(y =>
                {
                    y.Name = $"{i}: {c.Item} Stock: {c.Stock}";
                    y.Value = $"{c.Price}";
                    y.IsInline = true;
                });
            }

            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("buy", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task Buy(int item)
        {
            var user = Context.User;
            CheckInventoryExistence(user);

            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            var shoplist = GambleDB.Shoplist().ToList();

            if (shoplist[item] == null) return;
            if (userdata.Tokens >= shoplist[item].Price)
            {
                GambleDB.BuyItem(user, shoplist[item].Item);
                GambleDB.RemoveCredit(user, shoplist[item].Price);

                var embed = EmbedGenerator.DefaultEmbed(
                    $"{user.Username} bought {shoplist[item].Item} for ${shoplist[item].Price}", Colours.OKColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            else if (userdata.Tokens < shoplist[item].Price)
            {
                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You don't have enough money for that.",
                    Colours.FailColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("eventbuy", RunMode = RunMode.Async)]
        [Alias("ebuy")]
        [RequiredChannel(339383206669320192)]
        public async Task EventBuy(int item)
        {
            var user = Context.User;
            CheckInventoryExistence(user);

            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            var shoplist = GambleDB.EventShopList().ToList();

            if (shoplist[item] == null) return;
            if (item == 0 && shoplist[item].Stock == 0) return;
            if (userdata.Event_tokens >= shoplist[item].Price)
            {
                GambleDB.BuyItem(user, shoplist[item].Item);
                GambleDB.RemoveEventTokens(user, shoplist[item].Price);
                GambleDB.ChangeShopStockAmount();
                var embed = EmbedGenerator.DefaultEmbed(
                    $"{user.Username} bought {shoplist[item].Item} for {shoplist[item].Price} tokens",
                    Colours.OKColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
            else if (userdata.Event_tokens < shoplist[item].Price)
            {
                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You don't have enough for that.",
                    Colours.FailColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("createrole", RunMode = RunMode.Async)]
        [Alias("cr", "crole")]
        [RequiredChannel(339383206669320192)]
        public async Task CreateRole([Remainder] string name)
        {
            var user = Context.User as IGuildUser;

            var inventory = GambleDB.Inventory(user).FirstOrDefault();
            var roleStatus = GambleDB.CheckRoleStatus(user).FirstOrDefault();
            if (inventory.CustomRole > 0 && roleStatus != "yes")
            {
                GambleDB.UseItem(user, "CustomRole");
                GambleDB.UpdateRoleStatus(user);
                var role = await Context.Guild.CreateRoleAsync($"{name}", GuildPermissions.None);
                await user.AddRoleAsync(role);
                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} created role {name}", Colours.OKColour);
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed(
                    $"{user.Username}: You need to buy a role or you've already bought a role", Colours.FailColour);
                await ReplyAsync("", false, embed.Build());
            }
        }


        private static void CheckInventoryExistence(IUser user)
        {
            var inventory = GambleDB.Inventory(user);
            if (inventory == null)
                GambleDB.CreateInventory(user);
            else
                return;
        }
    }
}