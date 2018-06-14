using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Gambling.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;
using Microsoft.EntityFrameworkCore;

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
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(user);
                var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                                                       $"Event Tokens: {userData.EventTokens}", user.Mention,
                    Colours.DefaultColour, user);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }

        }

        [Command("wallet")]
        [Alias("balance", "money")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task Wallet(IGuildUser user)
        {
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(Context.User);
                var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                                                       $"Event Tokens: {userData.EventTokens}", user.Mention,
                    Colours.DefaultColour, user);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("Richest")]
        [Summary("Richest list")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Richest()
        {
            using (var db = new hanekawaContext())
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(Colours.DefaultColour));
                embed.Title = "Leaderboard";
                var result = db.Exp.OrderByDescending(x => x.Tokens).ToList();
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

        }

        [Command("reward")]
        [Alias("award")]
        [Summary("Rewards user with event tokens")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GiveCredit(int amount, IUser user)
        {
            if (amount == 0) return;
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
                userdata.EventTokens = userdata.EventTokens + Convert.ToUInt32(amount);
                await db.SaveChangesAsync();
                var content = $"{Context.User.Mention} awarded {amount} credit to {user.Mention}";
                var embed = EmbedGenerator.DefaultEmbed(content, Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}