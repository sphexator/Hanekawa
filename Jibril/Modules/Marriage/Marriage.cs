using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Fleet.Services;
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
            //TODO: Check if context.user is in DB, Return if user is in DB
            //TODO: Check if user is in DB, Return if user is in DB

            //TODO: Add User to DB with claim username
            await ReplyAsync($"{user.Mention}, do you take {Context.User.Mention} as your waifu/husbando? (Yes/No)");
            var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id));
            if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                await ReplyAsync($"{Context.User.Mention} has claimed {user.Mention} as her/his waifu/husbando!");
            }
        }
    }
}
