using Discord;
using Discord.Commands;
using Jibril.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Jibril.Modules.Game.Services;
using Jibril.Services;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using Jibril.Modules.Gambling.Services;

namespace Jibril.Modules.Game
{
    public class Consumable : ModuleBase<SocketCommandContext>
    {
        [Command("repair")]
        [RequiredChannel(346429281314013184)]
        public async Task SelfRepair()
        {
            var user = Context.User;
            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            var gamedata = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
            if (gamedata.Damagetaken > 0 && gamedata.Combatstatus < 1)
            {
                if (userdata.Tokens < 100)
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You don't have enough money to repair (Cost: $100)", Colours.DefaultColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    GambleDB.RemoveCredit(user, 100);
                    GameDatabase.Repair(user);

                    var embed = EmbedGenerator.DefaultEmbed($"{user.Username} paid $100 and got repaired back to full HP", Colours.DefaultColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed($"{user.Username} - You're either not damaged or in combat", Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("kit")]
        [RequiredChannel(346429281314013184)]
        public async Task UseKit()
        {
            var user = Context.User;
            var inventoryCheck = GambleDB.Inventory(user).FirstOrDefault();
            if (inventoryCheck == null)
            {
                GambleDB.CreateInventory(user);
            }
            var inventory = GambleDB.Inventory(user).FirstOrDefault();
            if (inventory.Repairkit > 0)
            {
                GambleDB.UseItem(user, "RepairKit");
                GameDatabase.Repair(user);

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithColor(new Color(0x4d006d));
                embed.Description = $"{user.Username} used a repair kit.";
                await ReplyAsync("", false, embed.Build());
            }
            else if (inventory.Repairkit <= 0)
            {
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithColor(new Color(211, 47, 47));
                embed.Description = $"{user.Username} - You do not have any repair kits on you";
                await ReplyAsync("", false, embed.Build());
            }

        }
    }
}
