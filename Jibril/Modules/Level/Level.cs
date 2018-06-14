using Discord;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Common;
using Jibril.Services.Level.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Level
{
    public class Level : ModuleBase<SocketCommandContext>
    {
        [Command("rank")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LevelChecker(IGuildUser user = null)
        {
            using (var db = new hanekawaContext())
            {
                if (user == null) user = Context.User as IGuildUser;
                var userData = await db.GetOrCreateUserData(user);
                var embed = EmbedGenerator.AuthorEmbed("", $"{user.Username}", Colours.DefaultColour, user);

                var level = new EmbedFieldBuilder
                {
                    Name = "Level",
                    IsInline = true,
                    Value = $"{userData.Level}"
                };
                var exp = new EmbedFieldBuilder
                {
                    Name = "Exp",
                    IsInline = true,
                    Value = $"{userData.Xp}/{Calculate.CalculateNextLevel(userData.Level)}"
                };
                var ranking = new EmbedFieldBuilder
                {
                    Name = "Rank",
                    IsInline = true,
                    Value = $"{await db.Exp.CountAsync(x => x.TotalXp >= userData.TotalXp)}/{await db.Exp.CountAsync()}"
                };

                embed.AddField(level);
                embed.AddField(exp);
                embed.AddField(ranking);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("top10")]
        [Alias("top")]
        [Remarks("Shows top10 on the leaderboard")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task Leaderboard()
        {
            using (var db = new hanekawaContext())
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(Colours.DefaultColour));
                embed.Title = "Leaderboard";
                var result = db.Exp.OrderByDescending(x => x.TotalXp).ToList();
                for (var i = 0; i < 10; i++)
                {
                    var c = result[i];
                    var rank = i + 1;
                    embed.AddField(y =>
                    {
                        y.Name = $"Rank {rank}";
                        y.Value = $"<@!{c.UserId}> | Level:{c.Level} - Total Exp:{c.TotalXp}";
                        y.IsInline = false;
                    });
                }
                await ReplyAsync(" ", false, embed.Build());
            }

        }

        [Command("give", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task GiveCredit(uint amount, IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new hanekawaContext())
            {
                var giver = await db.GetOrCreateUserData(Context.User);
                if (giver?.Tokens < amount)
                {
                    var failres = EmbedGenerator.DefaultEmbed("Not enough credit to give.", Colours.FailColour);
                    await ReplyAsync("", false, failres.Build());
                    return;
                }
                if (giver?.Level < 20)
                {
                    var failres = EmbedGenerator.DefaultEmbed("Not hight enough level to give people credit.", Colours.FailColour);
                    await ReplyAsync("", false, failres.Build());
                    return;
                }
                var reciever = await db.GetOrCreateUserData(user);
                reciever.Tokens = reciever.Tokens + amount;
                giver.Tokens = giver.Tokens - amount;
                await db.SaveChangesAsync();
                var embed = EmbedGenerator.DefaultEmbed(
                    $"{Context.User.Username} has given {user.Username} {amount} credit", Colours.DefaultColour);
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Daily()
        {
            using (var db = new hanekawaContext())
            {
                var user = Context.User as IGuildUser;
                var userData = await db.GetOrCreateUserData(user);
                
                var difference = DateTime.Compare(userData.Daily.Value, DateTime.Now);

                if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                    userData.Daily.Value.AddDays(1) <= DateTime.Now && difference < 0 || difference >= 0)
                {
                    userData.Daily = DateTime.Now;
                    if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                        userData.Daily.Value.AddDays(1) <= DateTime.Now && difference <= 0 || difference >= 0)
                    {
                        userData.Tokens = userData.Tokens + 200;
                        await ReplyAsync($"You received your daily $200!");
                    }
                }
                else
                {
                    var diff = DateTime.Now - userData.Daily.Value;
                    var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                    await ReplyAsync($"Your credits refresh in {di}!");
                }
            }

        }

        [Command("daily", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Daily(IGuildUser rewardUser)
        {
            if (rewardUser == Context.User) return;
            using (var db = new hanekawaContext())
            {
                var user = Context.User;
                var userData = await db.GetOrCreateUserData(user);
                var difference = DateTime.Compare(userData.Daily.Value, DateTime.Now);

                if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                    userData.Daily.Value.AddDays(1) <= DateTime.Now && difference < 0 || difference >= 0)
                {
                    userData.Daily = DateTime.Now;
                    if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                        userData.Daily.Value.AddDays(1) <= DateTime.Now && difference <= 0 || difference >= 0)
                    {
                        var reciever = await db.GetOrCreateUserData(rewardUser);
                        var rand = new Random();
                        var tokens = rand.Next(200, 400);
                        reciever.Tokens = reciever.Tokens + Convert.ToUInt32(new Random().Next(200,400));
                        await ReplyAsync($"You received your daily ${tokens}!");
                    }
                }
                else
                {
                    var diff = DateTime.Now - userData.Daily.Value;
                    var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                    await ReplyAsync($"Your credits refresh in {di}!");
                }
            }
        }

        [Command("rep", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Rep(IGuildUser rewardUser)
        {
            if (rewardUser == Context.User) return;
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(Context.User);

                var now = DateTime.Now;
                var daily = userData.Repcd;
                var difference = DateTime.Compare(daily, now);

                if (userData.Repcd.ToString() == "0001-01-01 00:00:00" ||
                    daily.AddDays(1) <= now && difference < 0 || difference >= 0)
                {
                    userData.Repcd = DateTime.Now;
                    if (userData.Repcd.ToString() == "0001-01-01 00:00:00" ||
                        daily.AddDays(1) <= now && difference <= 0 || difference >= 0)
                    {
                        var reciever = await db.GetOrCreateUserData(rewardUser);
                        reciever.Rep = reciever.Rep + 1;
                        await ReplyAsync($"{Context.User.Mention} gave a reputation point to {rewardUser.Mention}!");
                    }
                }
                else
                {
                    var diff = now - daily;
                    var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);
                    await ReplyAsync($"You can reward another rep in {di}!");
                }
            }
        }
    }
}