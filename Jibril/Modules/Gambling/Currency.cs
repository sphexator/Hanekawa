using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Gambling.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Gambling
{
    public class Currency : ModuleBase<SocketCommandContext>
    {
        [Command("wallet")]
        [Summary("Check user currency")]
        [Alias("balance", "money")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task Wallet()
        {
            var user = Context.User;
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                                                   $"Event Tokens: {userData.Event_tokens}", user.Mention,
                Colours.DefaultColour, user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("wallet")]
        [Alias("balance", "money")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task Wallet(IGuildUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                                                   $"Event Tokens: {userData.Event_tokens}", user.Mention,
                Colours.DefaultColour, user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("Richest")]
        [Summary("Richest list")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Richest()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(Colours.DefaultColour));
            embed.Title = "Leaderboard";
            var result = GambleDB.GetLeaderBoard().ToList();
            for (var i = 0; i < 10; i++)
            {
                var c = result[i];
                var rank = i + 1;
                embed.AddField(y =>
                {
                    y.Name = $"Rank {rank}";
                    y.Value = $"<@!{c.UserId}> | Credit:{c.Tokens}";
                    y.IsInline = false;
                });
            }

            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("reward")]
        [Summary("Rewards user with event tokens")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GiveCredit(int amount, IUser user)
        {
            if (amount == 0) return;

            GambleDB.AddEventCredit(user, amount);
            var content = $"{Context.User.Mention} awarded {amount} credit to {user.Mention}";
            var embed = EmbedGenerator.DefaultEmbed(content, Colours.DefaultColour);
            await ReplyAsync("", false, embed.Build());
        }
    }
}