using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Fleet.Services;
using Jibril.Modules.Marriage.Service;
using Jibril.Preconditions;

namespace Jibril.Modules.Marriage
{
    public class Marriage : InteractiveBase
    {
        [Command("waifu", RunMode = RunMode.Async)]
        [Alias("husbando")]
        [Summary("Ask someone to be your waifu or husbando")]
        public async Task ClaimWaifu(IGuildUser user)
        {
            var chchData = MarriageDb.MarriageData(Context.User.Id).FirstOrDefault();
            var chchUserData = MarriageDb.MarriageData(user.Id).FirstOrDefault();
            //TODO: Create response depending on which one is already taken
            if (chchData != null)
            {
                await ReplyAsync($"{Context.User.Mention}, you can't claim another waifu when you already have {chchData.ClaimName}!");
                return;
            }
            if (chchUserData != null)
            {
                await ReplyAsync($"{Context.User.Mention}, {user.Username} is already taken to {chchUserData.ClaimName}");
                return;
            }
            await ReplyAsync($"{Context.User.Mention} asks you to be his waifu/husbando. Would you share your life with him/her until the end ? (Yes/No)");
            var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id), TimeSpan.FromMinutes(2));
            if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                //TODO: Add User to DB with claim username
                await ReplyAsync($"I declare now {user.Mention} and {Context.User.Mention} as waifu and husbando!");
            }
            else if (response == null)
            {

            }
            else
            {

            }
        }
    }
}
