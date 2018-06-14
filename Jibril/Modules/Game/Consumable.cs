using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Gambling.Services;
using Jibril.Modules.Game.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Game
{
    public class Consumable : ModuleBase<SocketCommandContext>
    {
        [Command("repair")]
        [RequiredChannel(346429281314013184)]
        public async Task SelfRepair()
        {
            using (var db = new hanekawaContext())
            {
                var user = Context.User;
                var userdata = await db.GetOrCreateUserData(user);
                var gamedata = await db.GetOrCreateShipGame(user);
                if (gamedata.Damagetaken > 0 && gamedata.Combatstatus < 1)
                {
                    if (userdata.Tokens < 100)
                    {
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"{user.Username} - You don't have enough money to repair (Cost: $100)", Colours.DefaultColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        userdata.Tokens = userdata.Tokens - 100;
                        gamedata.Damagetaken = 0;
                        await db.SaveChangesAsync();

                        var embed = EmbedGenerator.DefaultEmbed(
                            $"{user.Username} paid $100 and got repaired back to full HP", Colours.DefaultColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You're either not damaged or in combat",
                        Colours.DefaultColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }

        }

        [Command("kit")]
        [RequiredChannel(346429281314013184)]
        public async Task UseKit()
        {
            using (var db = new hanekawaContext())
            {
                var user = Context.User;
                var inventory = await db.GetOrCreateInventory(user);
                if (inventory.RepairKit > 0)
                {
                    var gameData = await db.GetOrCreateShipGame(user);
                    inventory.RepairKit = inventory.RepairKit - 1;
                    gameData.Damagetaken = 0;
                    await db.SaveChangesAsync();

                    var embed = new EmbedBuilder();
                    embed.WithColor(new Color(0x4d006d));
                    embed.Description = $"{user.Username} used a repair kit.";
                    await ReplyAsync("", false, embed.Build());
                }
                else if (inventory.RepairKit <= 0)
                {
                    var embed = new EmbedBuilder();
                    embed.WithColor(new Color(211, 47, 47));
                    embed.Description = $"{user.Username} - You do not have any repair kits on you";
                    await ReplyAsync("", false, embed.Build());
                }
            }

        }
    }
}