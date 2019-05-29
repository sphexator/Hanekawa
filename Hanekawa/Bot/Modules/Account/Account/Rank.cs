﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account
{
    [Name("Account")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Account : InteractiveBase
    {
        private readonly ImageGenerator _image;
        private readonly ExpService _exp;
        public Account(ExpService exp, ImageGenerator image)
        {
            _exp = exp;
            _image = image;
        }

        [Name("Rank")]
        [Command("rank")]
        [Description("Display your rank, level and exp within the server, also global rank")]
        [Remarks("rank")]
        [RequiredChannel]
        public async Task RankAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                if (user == null) user = Context.User;
                var serverData = await db.GetOrCreateUserData(user);
                var globalData = await db.GetOrCreateGlobalUserData(user);
                var embed = new EmbedBuilder().CreateDefault("", Context.Guild.Id);
                embed.Author = new EmbedAuthorBuilder {Name = user.GetName()};
                embed.ThumbnailUrl = user.GetAvatar();
                embed.Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder {Name = "Level", Value = $"{serverData.Level}", IsInline = true},
                    new EmbedFieldBuilder
                        {Name = "Exp", Value = $"{serverData.Exp}/{_exp.ExpToNextLevel(serverData)}", IsInline = true},
                    new EmbedFieldBuilder
                    {
                        Name = "Rank",
                        Value =
                            $"{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id && x.TotalExp >= serverData.TotalExp)}" +
                            $"/{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id)}",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Global Rank",
                        Value =
                            $"{await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp)}" +
                            $"/{await db.AccountGlobals.CountAsync()}",
                        IsInline = true
                    }
                };
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Level Leaderboard")]
        [Command("top", "leaderboard", "lb")]
        [Description("Displays highest ranked users")]
        [Remarks("leaderboard")]
        [RequiredChannel]
        public async Task LeaderboardAsync(int amount = 50)
        {
            using (var db = new DbService())
            {
                var toGet = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
                var users = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id).Take(toGet).ToArrayAsync();
                var result = new List<string>();
                for (var i = 0; i < users.Length; i++)
                {
                    var strBuilder = new StringBuilder();
                    var user = users[i];
                    var username = Context.Guild.GetUser(user.UserId);
                    strBuilder.AppendLine($"**Rank: {i + 1}** - {username.Mention ?? $"User left server({user.UserId})"}");
                    strBuilder.AppendLine($"-> Level:{user.Level} - Total Exp: {user.TotalExp}");
                    result.Add(strBuilder.ToString());
                }

                await PagedReplyAsync(
                    result.PaginateBuilder(Context.Guild, $"Leaderboard for {Context.Guild.Name}", null, 10));
            }
        }

        [Name("Reputation")]
        [Command("rep")]
        [Description("Rewards a reputation to a user. Usable once a day")]
        [Remarks("rep @bob#0000")]
        public async Task RepAsync(SocketGuildUser user = null)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
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
                            $"{Context.User.Mention}, you got a reputation point available!",
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

                var userData = await db.GetOrCreateUserData(user);
                cooldownCheckAccount.RepCooldown = DateTime.UtcNow;
                userData.Rep++;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"rewarded {user.Mention} with a reputation point!", Color.Green.RawValue);
            }
        }
    }
}