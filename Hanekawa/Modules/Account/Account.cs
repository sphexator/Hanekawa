﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Modules.Account.Profile;
using Hanekawa.Preconditions;
using Hanekawa.Services.Level.Util;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Account
{
    public class Account : InteractiveBase
    {
        private readonly Calculate _calculate;
        private readonly ProfileGenerator _profileBuilder;
        private readonly DbService _db;

        public Account(Calculate calculate, ProfileGenerator profileBuilder, DbService db)
        {
            _calculate = calculate;
            _profileBuilder = profileBuilder;
            _db = db;
        }

        [Command("rank", RunMode = RunMode.Async)]
        [Summary("Displays your rank")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task RankAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;

            var userdata = await _db.GetOrCreateUserData(user);
            var glUserData = await _db.GetOrCreateGlobalUserData(user);
            var rank = _db.Accounts.CountAsync(x =>
                x.GuildId == Context.Guild.Id && x.TotalExp >= userdata.TotalExp && x.Active);
            var total = _db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id);

            var globalRank = _db.AccountGlobals.CountAsync();
            var globalUserRank = _db.AccountGlobals.CountAsync(x => x.TotalExp >= glUserData.TotalExp);

            await Task.WhenAll(rank, total, globalRank, globalUserRank);
            var nxtLevel = _calculate.GetServerLevelRequirement(userdata.Level);
            await Context.ReplyAsync(new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder {Name = user.GetName()},
                Color = Color.Purple,
                ThumbnailUrl = user.GetAvatar(),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {IsInline = true, Name = "Level", Value = $"{userdata.Level}"},
                    new EmbedFieldBuilder {IsInline = true, Name = "Exp", Value = $"{userdata.Exp}/{nxtLevel}"},
                    new EmbedFieldBuilder {IsInline = true, Name = "Rank", Value = $"{rank.Result}/{total.Result}"},
                    new EmbedFieldBuilder
                        {IsInline = true, Name = "Global Rank", Value = $"{globalUserRank.Result}/{globalRank.Result}"}
                }
            });
        }

        [Command("top", RunMode = RunMode.Async)]
        [Summary("Displays top 10 users on the level leaderboard")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task LeaderboardAsync()
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
            var users = await _db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                .OrderByDescending(account => account.TotalExp).Take(10).ToListAsync();
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

            await Context.ReplyAsync(embed);
        }

        [Command("rep", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task RepAsync(SocketGuildUser user = null)
        {
            if (user == Context.User) return;
            var cooldownCheckAccount = await _db.GetOrCreateUserData(Context.User as SocketGuildUser);
            if (user == null)
            {
                if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                    await Context.ReplyAsync(
                        $"{Context.User.Mention} daily rep refresh in {timer.Humanize()}", Color.Red.RawValue);
                }
                else
                {
                    await Context.ReplyAsync(
                        $"{Context.User.Mention}, you got a reputation point point available!",
                        Color.Green.RawValue);
                }

                return;
            }
            if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
            {
                var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                await Context.ReplyAsync($"{Context.User.Mention} daily rep refresh in {timer.Humanize()}",
                    Color.Red.RawValue);
                return;
            }
            var userdata = await _db.GetOrCreateUserData(user);
            cooldownCheckAccount.RepCooldown = DateTime.UtcNow;
            userdata.Rep = userdata.Rep + 1;
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"rewarded {user.Mention} with a reputation point!", Color.Green.RawValue);
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
        /*
         <-- MAY USE THIS FOR LATER -->
         TODO: PREVIEW, MAYBE USE
        [Command("preview", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel]
        public async Task PreviewProfileAsync(string url)
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                using (var stream = await _profileBuilder.Preview(Context.User as SocketGuildUser, url))
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
                            $"{Context.User.Mention} couldn't make a background with that picture. Please use a direct image and preferably from imgur.com",
                            Color.Red.RawValue)
                        .Build());
            }
        }
        */
    }
}