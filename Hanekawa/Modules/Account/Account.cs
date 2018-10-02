using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Modules.Account.Profile;
using Hanekawa.Preconditions;
using Hanekawa.Services.Level.Util;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account
{
    public class Account : InteractiveBase
    {
        private readonly Calculate _calculate;
        private readonly ProfileGenerator _profileBuilder;

        public Account(Calculate calculate, ProfileGenerator profileBuilder)
        {
            _calculate = calculate;
            _profileBuilder = profileBuilder;
        }

        [Command("rank", RunMode = RunMode.Async)]
        [Summary("Displays your rank")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task RankAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var rank = db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id && x.TotalExp >= userdata.TotalExp && x.Active);
                var total = db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id);
                await Task.WhenAll(rank, total);
                var nxtLevel = _calculate.GetServerLevelRequirement(userdata.Level);

                var author = new EmbedAuthorBuilder
                {
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
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
        [Summary("Displays top 10 users on the level leaderboard")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var author = new EmbedAuthorBuilder
                {
                    Name = $"Level leaderboard for {Context.Guild.Name}",
                    IconUrl = Context.Guild.IconUrl
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author
                };
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id).OrderByDescending(account => account.TotalExp).Take(10).ToListAsync();
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
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task RepAsync(SocketGuildUser user = null)
        {
            if (user == Context.User) return;
            if (user == null)
            {
                using (var db = new DbService())
                {
                    var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                    {
                        var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"{Context.User.Mention} daily rep refresh in {timer.Humanize()}",
                                Color.Red.RawValue).Build());
                    }
                    else
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply($"{Context.User.Mention}, you got a reputation point point available!",
                                    Color.Green.RawValue).Build()
                        );
                }

                return;
            }
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} daily rep refresh in {timer.Humanize()}", Color.Red.RawValue).Build());
                    return;
                }
                var userdata = await db.GetOrCreateUserData(user);
                cooldownCheckAccount.RepCooldown = DateTime.UtcNow;
                userdata.Rep = userdata.Rep + 1;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"rewarded {user?.Mention} with a reputation point!", Color.Green.RawValue).Build());
            }
        }

        [Command("profile", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task ProfileAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            await Context.Channel.TriggerTypingAsync();
            using (var stream = await _profileBuilder.Create(user))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(stream, "profile.png");
            }
        }

        [Command("preview", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task PreviewProfileAsync(string url)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                using (var stream = await _profileBuilder.Preview((Context.User as SocketGuildUser), url))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await Context.Channel.SendFileAsync(stream, "profile.png");
                }
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"{Context.User.Mention} couldn't make a background with that picture. Please use a direct image and preferably from imgur.com", Color.Red.RawValue)
                        .Build());
            }
        }
    }
}