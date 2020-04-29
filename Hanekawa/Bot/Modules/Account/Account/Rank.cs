using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account
{
    [Name("Account")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public partial class Account : HanekawaModule
    {
        private readonly ExpService _exp;
        private readonly ImageGenerator _image;

        public Account(ExpService exp, ImageGenerator image)
        {
            _exp = exp;
            _image = image;
        }

        [Name("Rank")]
        [Command("rank")]
        [Description("Display your rank, level and exp within the server, also global rank")]
        [RequiredChannel]
        public async Task RankAsync(CachedMember user = null)
        {
            using var db = new DbService();
            if (user == null) user = Context.Member;
            var serverData = await db.GetOrCreateUserData(user);
            var globalData = await db.GetOrCreateGlobalUserData(user);
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {Name = user.DisplayName},
                ThumbnailUrl = user.GetAvatarUrl(),
                Fields =
                {
                    new LocalEmbedFieldBuilder {Name = "Level", Value = $"{serverData.Level}", IsInline = true},
                    new LocalEmbedFieldBuilder
                    {
                        Name = "Exp",
                        Value = $"{serverData.Exp}/{_exp.ExpToNextLevel(serverData)}",
                        IsInline = true
                    },
                    new LocalEmbedFieldBuilder
                    {
                        Name = "Rank",
                        Value =
                            $"{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue && x.TotalExp >= serverData.TotalExp)}" +
                            $"/{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue)}",
                        IsInline = true
                    },
                    new LocalEmbedFieldBuilder
                    {
                        Name = "Global Rank",
                        Value =
                            $"{await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp)}" +
                            $"/{await db.AccountGlobals.CountAsync()}",
                        IsInline = true
                    }
                }
            };
            await Context.ReplyAsync(embed);
        }

        [Name("Level Leaderboard")]
        [Command("top", "leaderboard", "lb")]
        [Description("Displays highest ranked users")]
        [RequiredChannel]
        public async Task LeaderboardAsync(int amount = 50)
        {
            using (var db = new DbService())
            {
                var toGet = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
                var users = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id.RawValue).OrderByDescending(x => x.TotalExp).Take(toGet).ToArrayAsync();
                var result = new List<string>();
                var strBuilder = new StringBuilder();
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var username = Context.Guild.GetMember(user.UserId);
                    strBuilder.AppendLine(
                        username != null
                            ? $"**Rank: {i + 1}** - {username.Mention}"
                            : $"**Rank: {i + 1}** - User left server({user.UserId})");
                    strBuilder.Append($"-> Level:{user.Level} - Total Exp: {user.TotalExp}");
                    result.Add(strBuilder.ToString());
                    strBuilder.Clear();
                }

                await Context.PaginatedReply(result, Context.Guild, $"Leaderboard for {Context.Guild.Name}");
            }
        }

        [Name("Reputation")]
        [Command("rep")]
        [Description("Rewards a reputation to a user. Usable once a day")]
        [Remarks("rep @bob#0000")]
        public async Task RepAsync(CachedMember user = null)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.Member);
                if (user == null)
                {
                    if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                    {
                        var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                        await Context.ReplyAsync(
                            $"{Context.User.Mention} daily rep refresh in {timer.Humanize(2)}");
                    }
                    else
                    {
                        await Context.ReplyAsync(
                            $"{Context.User.Mention}, you got a reputation point available!",
                            Color.Green);
                    }

                    return;
                }

                if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                    await Context.ReplyAsync($"{Context.User.Mention} daily rep refresh in {timer.Humanize(2)}",
                        Color.Red);
                    return;
                }

                var userData = await db.GetOrCreateUserData(user);
                cooldownCheckAccount.RepCooldown = DateTime.UtcNow;
                userData.Rep++;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"rewarded {user.Mention} with a reputation point!", Color.Green);
            }
        }
    }
}