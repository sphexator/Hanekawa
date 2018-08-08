using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;

namespace Hanekawa.Modules.Account.Shop
{
    public class Shop : InteractiveBase
    {
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [RequiredChannel]
        public async Task InventoryAsync()
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Inventory is currently disabled", Color.Red.RawValue).Build());
        }

        [Command("shop", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task ShopAsync()
        {
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Shop is currently disabled").Build());
        }

        [Command("buy", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task BuyAsync(uint itemId)
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Buy command is currently disabled for rework.", Color.Red.RawValue).Build());
        }
    }
}
