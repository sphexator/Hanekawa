using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Marriage.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Marriage
{
    public class Marriage : InteractiveBase
    {
        private readonly MarriageService _marriageService;
        public Marriage(MarriageService marriageService)
        {
            _marriageService = marriageService;
        }

        [Command("marry", RunMode = RunMode.Async)]
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
                _marriageService.AddWaifu((Context.User as IGuildUser), user);
                await ReplyAsync($"I declare now {user.Mention} and {Context.User.Mention} as waifu and husbando!");
            }
        }

        [Command("divorce", RunMode = RunMode.Async)]
        [Summary("Divorce your waifu/husbando")]
        public async Task Divorce()
        {
            var data = MarriageDb.MarriageData(Context.User.Id).FirstOrDefault();
            if (data == null)
            {
                await ReplyAsync($"{Context.User.Username}, you're currently not married");
                return;
            }
            await ReplyAsync($"You sure you want to divorce {data.ClaimName}?");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));
            if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ||
                response.Content.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                _marriageService.RemoveWaifu(Context.User as IGuildUser);
                await ReplyAsync($"{Context.User.Mention} has divorced <@{data.Claim}>");
            }
        }
    }
}