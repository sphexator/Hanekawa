using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Account
{
    public class Economy : InteractiveBase
    {
        [Command("wallet")]
        [Alias("balance", "money")]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            if(user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.DarkPurple,
                    Description = $"Credit: ${userdata.Credit}\n" +
                                  $"Event Credit: ${userdata.CreditSpecial}"
                };
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("give", RunMode = RunMode.Async)]
        [Alias("transfer")]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task GiveCreditAsync(uint amount, SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var recieverData = await db.GetOrCreateUserData(user);
                if (userdata.Credit <= amount)
                {
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have enough credit", Color.Red.RawValue).Build());
                    return;
                }

                userdata.Credit = userdata.Credit - amount;
                recieverData.Credit = recieverData.Credit + amount;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} transferred ${amount} to {user.Mention}", Color.Red.RawValue).Build());
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task DailyAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} daily credit refresh in {timer.Humanize()}", Color.Red.RawValue).Build());
                    return;
                }

                uint reward;
                if (user == null)
                {
                    user = Context.User as SocketGuildUser;
                    reward = 200;
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.DailyCredit = DateTime.UtcNow;
                    userdata.Credit = userdata.Credit + reward;
                    await db.SaveChangesAsync();
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"rewarded {user?.Mention} with ${reward} credit", Color.Green.RawValue).Build());
                }
                else
                {
                    reward = Convert.ToUInt32(new Random().Next(200, 300));
                    var userData = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var recieverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    recieverData.Credit = recieverData.Credit + reward;
                    await db.SaveChangesAsync();
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} rewarded {user.Mention} with ${reward} credit", Color.Green.RawValue).Build());
                }
            }
        }

        [Command("richest", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var embed = new EmbedBuilder
                {
                    Color = Color.DarkPurple,
                    Title = "Leaderboard"
                };
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id).OrderByDescending(account => account.Credit).Take(10).ToListAsync();
                var rank = 1;
                foreach (var x in users)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value = $"<@{x.UserId}> - Credit:{x.Credit}"
                    };
                    embed.AddField(field);
                    rank++;
                }
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("reward", RunMode = RunMode.Async)]
        [Alias("award")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RewardCreditAsync(uint amount, IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                userdata.CreditSpecial = userdata.CreditSpecial + amount;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Rewarded ${amount} Event Credit to {user.Mention}").Build());
            }
        }
    }
}