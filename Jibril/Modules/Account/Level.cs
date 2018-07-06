using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Jibril.Services.Level.Services;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Account
{
    public class Level : InteractiveBase
    {
        private readonly Calculate _calculate;

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
                    Value = $"{rank}/{total}"
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
                        Value = $"<@{x.UserId} - Level:{x.Level} - Total Exp:{x.TotalExp}"
                    };
                    embed.AddField(field);
                    rank++;
                }
            }
        }
    }
}
