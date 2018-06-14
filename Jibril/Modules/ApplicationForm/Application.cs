using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Preconditions;
using Jibril.Services;

namespace Jibril.Modules.ApplicationForm
{
    public class Application : InteractiveBase
    {
        /*
        [Command("apply", RunMode = RunMode.Async)]
        [RequireOwner]
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
                                 "Question 1/5 \n" +
                                 "How old are you?");
                var age = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                if (age != null)
                {
                    await ReplyAsync("Question 2/5 \n" +
                                     "Which of the following positions are you applying for (mention multiple if applying for multiple): \n" +
                                     "1. Moderator. \n" +
                                     "2. Event. \n" +
                                     "3. Art / Design.");
                    var position = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                    if (position != null)
                    {
                        await ReplyAsync("Question 3/5\n" +
                                         "Region and/or timezone?");
                        var region = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                        if (region != null)
                        {
                            await ReplyAsync("Question 4/5 \n" +
                                             "Tell us briefly about yourself.");
                            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                            if (response != null)
                            {
                                await ReplyAsync("Question 5/5 \n" +
                                                 "Do you have any previous experience or portofolio for design (not needed)?\n" +
                                                 "\n" +
                                                 "Prioritize experience on discord but name any you got");
                                var experience = await NextMessageAsync(true, true, TimeSpan.FromMinutes(15));

                                var totalMessageCount = $"**Age:** {age}\n" +
                                                        $"**Position**\n" +
                                                        $"{position}\n" +
                                                        $"**Region/Timezone:**\n" +
                                                        $"{region}\n" +
                                                        $"\n" +
                                                        $"**About:**\n" +
                                                        $"{response}\n" +
                                                        $"\n" +
                                                        $"**Experience:**\n" +
                                                        $"{experience}";

                                if (experience != null && totalMessageCount.Length <= 2000)
                                {
                                    var embed = new EmbedBuilder();
                                    var author = new EmbedAuthorBuilder();
                                    var footer = new EmbedFooterBuilder();

                                    author.IconUrl = Context.User.GetAvatarUrl();
                                    author.Name = $"{Context.User.Username}#{Context.User.Discriminator}";

                                    footer.WithText($"{DateTime.Now}");

                                    embed.Description = totalMessageCount;
                                    embed.Color = new Color(Colours.DefaultColour);

                                    embed.WithAuthor(author);
                                    embed.WithFooter(footer);
                                    var guild = Context.Client.GetGuild(339370914724446208);
                                    var log = guild.GetChannel(366672975153594378) as ITextChannel;
                                    await log.SendMessageAsync("", false, embed.Build());
                                    await ReplyAsync($"Application sent.");
                                }
                                else
                                {
                                    await ReplyAsync("Timed out");
                                }
                            }
                            else
                            {
                                await ReplyAsync("Timed out");
                            }
                        }
                        else
                        {
                            await ReplyAsync("Timed out");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Timed out");
                    }
                }
                else
                {
                    await ReplyAsync("Timed out");
                }
            }
            else
            {
                await ReplyAsync("You need to be level10 or higher inorder to apply.");
            }
        }
        */
    }
}