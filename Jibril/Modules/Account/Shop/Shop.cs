using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;
using System.Threading.Tasks;

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
    }
}
