using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.Addons.Preconditions;

namespace Jibril.Modules.ApplicationForm
{
    public class Application : InteractiveBase
    {
        [Command("apply", RunMode = RunMode.Async)]
        [Ratelimit(1, 60, Measure.Minutes, false, true)]
        [RequireContext(ContextType.DM)]
        [Priority(0)]
        public async Task ApplicationProcess()
        {
            var userData = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userData.Level >= 10)
            {


                await ReplyAsync("Application process started.\n" +
                                    "Required userdata collected." +
                                    "\n" +
                                    "\n" +
                                    "Keep in mind, when you answer the questions, you need to keep them all in one(1) message." +
                                    "\n" +
                                    "\n" +
                                    "Question 1/3 \n" +
                                    "How old are you?");
                var age = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                if (age != null)
                {
                    await ReplyAsync("Question 2/3 \n" +
                                        "Tell us a bit about yourself.");
                    var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                    if (response != null & age != null)
                    {
                        await ReplyAsync("Question 3/3 \n" +
                                            "Do you have any previous moderation experience?\n" +
                                            "\n" +
                                            "Prioritize experience on discord but name any you got");
                        var experience = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                        if (experience != null && response != null && age != null)
                        {
                            await ReplyAsync($"Application sent.");
                            EmbedBuilder embed = new EmbedBuilder();
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                            EmbedFooterBuilder footer = new EmbedFooterBuilder();

                            author.IconUrl = Context.User.GetAvatarUrl();
                            author.Name = Context.User.Username;
                            footer.WithText($"{DateTime.Now}");

                            embed.Description = $"Age:{age}\n" +
                                                $"\n" +
                                                $"Tell us a bit about yourself:\n" +
                                                $"{response}\n" +
                                                $"\n" +
                                                $"Experience:\n" +
                                                $"{experience}";

                            embed.WithAuthor(author);
                            embed.WithFooter(footer);

                            var guild = Context.Client.GetGuild(339370914724446208);

                            var log = guild.GetChannel(366672975153594378) as ITextChannel;
                            await log.SendMessageAsync("", false, embed.Build());

                        }
                        else
                        {
                            await ReplyAsync("Timed out");
                            return;
                        }
                    }
                    else
                    {
                        await ReplyAsync("Timed out");
                        return;
                    }
                }
                else
                {
                    await ReplyAsync("Timed out");
                    return;
                }
            }
            else
            {
                await ReplyAsync("You need to be level10 or higher inorder to apply.");
            }
        }
    }
}
