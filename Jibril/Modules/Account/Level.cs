using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Jibril.Services.Level.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Account
{
    public class Level : InteractiveBase
    {
        private readonly Calculate _calculate;

        public Level(Calculate calculate)
        {
            _calculate = calculate;
        }

        [Command("rank", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task RankAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var rank = db.Accounts.CountAsync(x => x.TotalExp >= userdata.TotalExp);
                var total = db.Accounts.CountAsync();
                await Task.WhenAll(rank, total);
                var nxtLevel = _calculate.GetNextLevelRequirement(userdata.Level);
                var author = new EmbedAuthorBuilder
                {
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.DarkPurple,
                    Author = author,
                    ThumbnailUrl = user.GetAvatar()
                };
                var level = new EmbedFieldBuilder
                {
                    Name = "Level",
                    IsInline = true,
                    Value = $"{userdata.Level}"
                };
                var exp = new EmbedFieldBuilder
                {
                    Name = "Exp",
                    IsInline = true,
                    Value = $"{userdata.Exp}/{nxtLevel}"
                };
                var ranking = new EmbedFieldBuilder
                {
                    Name = "Rank",
                    IsInline = true,
                    Value = $"{rank.Result}/{total.Result}"
                };

                embed.AddField(level);
                embed.AddField(exp);
                embed.AddField(ranking);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("top", RunMode = RunMode.Async)]
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
                var users = await db.Accounts.Where(x => x.Active).OrderByDescending(account => account.TotalExp).Take(10).ToListAsync();
                var rank = 1;
                foreach (var x in users)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value = $"<@{x.UserId}> - Level:{x.Level} - Total Exp:{x.TotalExp}"
                    };
                    embed.AddField(field);
                    rank++;
                }
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("rep", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task RepAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} daily rep refresh in {timer.Humanize()}", Color.Red.RawValue).Build());
                    return;
                }
                var userdata = await db.GetOrCreateUserData(user);
                userdata.RepCooldown = DateTime.UtcNow;
                userdata.Rep = userdata.Rep + 1;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"rewarded {user?.Mention} with a reputation point!", Color.Green.RawValue).Build());
            }
        }
    }
}