using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Gambling.Services;
using Jibril.Modules.Profile.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Profile
{
    public class SetProfile : InteractiveBase
    {
        [Command("setbackground")]
        [Alias("setb", "sb")]
        [Summary("Sets a background for your profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task SetProfileURL(string url)
        {
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(Context.User);
                await Context.Message.DeleteAsync();
                if (userdata.Tokens >= 5000)
                {
                    userdata.Tokens = userdata.Tokens - 5000;
                    userdata.Profilepic = url;
                    var embed = EmbedGenerator.DefaultEmbed("Purchased and applied background", Colours.OkColour);
                    await ReplyAndDeleteAsync($"", false, embed.Build(), TimeSpan.FromSeconds(15));
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed("You don't have enough money to change background",
                        Colours.FailColour);
                    await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15));
                }
            }
        }

        [Command("removebackground")]
        [Alias("remb", "rb")]
        [Summary("Removes background you've set for your profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task RemoveProfileURL(string url)
        {
            var user = Context.User;
            var pfp = ProfileDB.CheckProfilePicture(user).FirstOrDefault();
            await Context.Message.DeleteAsync();

            if (pfp != "o")
            {
                ProfileDB.RemoveProfileURL(user);
                var embed = EmbedGenerator.DefaultEmbed("Removed background", Colours.OkColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10));
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed("You currently don't have a background", Colours.FailColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(10));
            }
        }
    }
}