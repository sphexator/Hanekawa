using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.ImageGeneration;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Account")]
    [Description("Commands for user levels")]
    [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.AttachFiles | Permission.SendMessages)]
    public class Account : HanekawaCommandModule
    {
        private readonly ExpService _exp;
        private readonly ImageGenerationService _image;

        public Account(ImageGenerationService image, ExpService exp)
        {
            _image = image;
            _exp = exp;
        }

        [Name("Rank")]
        [Command("rank")]
        [Description("Display your rank, level and exp within the server, also global rank")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task<DiscordResponseCommandResult> RankAsync(IMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            user ??= Context.Author;
            var serverData = await db.GetOrCreateUserData(user);
            var globalData = await db.GetOrCreateGlobalUserDataAsync(user);
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {Name = user.DisplayName()},
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
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
                            $"{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id && x.TotalExp >= serverData.TotalExp  && x.Active)}" +
                            $"/{await db.Accounts.CountAsync(x => x.GuildId == Context.Guild.Id)}",
                        IsInline = false
                    },
                    new LocalEmbedFieldBuilder
                    {
                        Name = "Global Rank",
                        Value =
                            $"{await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp)}" +
                            $"/{await db.AccountGlobals.CountAsync()}",
                        IsInline = false
                    }
                }
            };
            return Reply(embed);
        }

        [Name("Profile")]
        [Command("profile")]
        [Description("Showcase yours or another persons profile")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task<DiscordCommandResult> ProfileAsync(IMember user = null)
        {
            // TODO: Look into
            user ??= Context.Author;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var image = await _image.ProfileBuilder(user, db);
            return Reply(new LocalMessageBuilder {Attachments = {new LocalAttachment(image, "profile.png")}});
        }

        [Name("Level Leaderboard")]
        [Command("top", "leaderboard", "lb")]
        [Description("Displays highest ranked users")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task<DiscordMenuCommandResult> LeaderboardAsync(int amount = 100)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var toGet = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
            var users = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id.RawValue && x.Active)
                .OrderByDescending(x => x.TotalExp - x.Decay).Take(toGet).ToArrayAsync();
            var result = new List<string>();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < users.Length; i++)
            {
                var user = users[i];
                var username = await Context.Guild.GetOrFetchMemberAsync(user.UserId);
                if (username == null)
                {
                    user.Active = false;
                    continue;
                }
                strBuilder.AppendLine($"**Rank: {i + 1}** - {username.Mention}");
                strBuilder.Append($"-> Level:{user.Level} - Total Exp: {user.TotalExp}");
                result.Add(strBuilder.ToString());
                strBuilder.Clear();
            }

            await db.SaveChangesAsync();
            return Pages(result.PaginationBuilder(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"Leaderboard for {Context.Guild.Name}", 10));
        }

        [Name("Reputation")]
        [Command("rep")]
        [Description("Rewards a reputation to a user. Usable once a day")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [Remarks("rep @bob#0000")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> ReputationAsync(IMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cdCheck = await db.GetOrCreateUserData(Context.Author);
            if (user == null || user == Context.Author)
            {
                if (cdCheck.RepCooldown.Date.AddDays(1) > DateTime.UtcNow)
                {
                    var timer = cdCheck.RepCooldown.Date.AddDays(1) - DateTime.UtcNow;
                    await Reply(
                        $"{Context.Author.Mention} daily rep refresh in {timer.Humanize(2)}\n" +
                        "Reputation reset at midnight UTC!");
                }
                else
                {
                    return Reply(
                        $"{Context.Author.Mention}, you got a reputation point available!",
                        HanaBaseColor.Ok());
                }
            }
            
            if (cdCheck.RepCooldown.Date.AddDays(1) > DateTime.UtcNow)
            {
                var timer = cdCheck.RepCooldown.Date.AddDays(1) - DateTime.UtcNow;
                return Reply($"{Context.Author.Mention} daily rep refresh in {timer.Humanize(2)}\n" +
                                         "Reputation reset at midnight UTC!",
                    HanaBaseColor.Bad());
            }

            var userData = await db.GetOrCreateUserData(user);
            cdCheck.RepCooldown = DateTime.UtcNow.Date;
            userData.Rep++;
            await db.SaveChangesAsync();
            return Reply($"Rewarded {user?.Mention} with a reputation point!", Color.Green);
        }
    }
}