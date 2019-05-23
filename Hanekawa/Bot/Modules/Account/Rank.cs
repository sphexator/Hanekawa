using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account
{
    [Name("Account")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Account : InteractiveBase
    {
        private readonly ExpService _exp;
        public Account(ExpService exp) => _exp = exp;

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
        [Command("top", "leaderboard")]
        [Description("Displays highest ranked users")]
        [Remarks("top")]
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
    }
}