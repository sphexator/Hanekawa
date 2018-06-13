using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Game.Services;
using Jibril.Preconditions;
using Jibril.Services;

namespace Jibril.Modules.Game
{
    public class ClassChange : InteractiveBase
    {
        [Command("class", RunMode = RunMode.Async)]
        [RequiredChannel(346429281314013184)]
        public async Task ChangeShipClass()
        {
            var user = Context.User;
            try
            {
                var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();
                if (gameData.Combatstatus != 1)
                    try
                    {
                        var userData = DatabaseService.UserData(user).FirstOrDefault();
                        var embed = new EmbedBuilder
                        {
                            Color = new Color(0x4d006d)
                        };
                        var result = GameDatabase.GetClasses().ToList();
                        var amount = ClassChangeService.EligibleClasses(userData.Level);
                        for (var i = 0; i < amount; i++)
                        {
                            var c = result[i];
                            embed.AddField(y =>
                            {
                                y.Name = $"{i + 1}";
                                y.Value = $"{c.Class}       Level:{c.Level}";
                                y.IsInline = false;
                            });
                        }

                        await ReplyAsync($"{user.Username} - Which class would you want to switch to?\n" +
                                         $"Respond with name provided.", false, embed.Build()).ConfigureAwait(false);
                        var response = await NextMessageAsync().ConfigureAwait(false);
                        if (response != null)
                        {
                            var eligible = ClassChangeService.ChangeEligibility(response.Content, userData.Level);
                            if (eligible == 1)
                            {
                                GameDatabase.ChangeShipClass(user, response.Content);
                                await ReplyAsync($"{user.Username} successfully changed to {response.Content}")
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                await ReplyAsync($"{user.Username}, you're not qualified for that class yet.")
                                    .ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await ReplyAsync("You did not reply before the timeout").ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                    }
                else
                    await ReplyAsync($"{user.Username} - You're currently in combat and cannot change class. \n" +
                                     $"use **!flee** if you don't don't want to finish the fight.")
                        .ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }
}