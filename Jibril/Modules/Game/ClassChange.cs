using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Extensions;
using Jibril.Modules.Game.Services;
using Jibril.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Game
{
    public class ClassChange : InteractiveBase
    {
        [Command("class", RunMode = RunMode.Async)]
        [RequiredChannel(346429281314013184)]
        public async Task ChangeShipClass()
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                try
                {
                    var gameData = await db.GetOrCreateShipGame(user);
                    if (gameData.Combatstatus != 1)
                        try
                        {
                            var userData = await db.GetOrCreateUserData(user);
                            var embed = new EmbedBuilder
                            {
                                Color = new Color(0x4d006d)
                            };
                            var result = db.Classes.OrderBy(x => x.Level).ToList();
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
                                    userData.ShipClass = response.Content;
                                    await db.SaveChangesAsync();
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
                                await ReplyAsync("You did not reply in time").ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    else
                        await ReplyAsync($"{user.Username} - You're currently in combat and cannot change class. \n" +
                                         $"use **!flee** if you don't don't want to finish the fight.")
                            .ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }

        }
    }
}