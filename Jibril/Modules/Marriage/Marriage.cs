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
        [Command("waifu")]
        [Alias("husbando")]
        [Summary("Ask someone to be your waifu or husbando")]
        public async Task ClaimWaifu(IGuildUser user)
        {
            var chchData = MarriageDb.MarriageData(Context.User).FirstOrDefault();
            var chchUserData = MarriageDb.MarriageData(user).FirstOrDefault();
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
            await ReplyAsync($"{user.Mention}, do you take {Context.User.Mention} as your waifu/husbando? (Yes/No)");
            var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id));
            if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                //TODO: Add User to DB with claim username
                await ReplyAsync($"{Context.User.Mention} has claimed {user.Mention} as her/his waifu/husbando!");
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
