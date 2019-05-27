using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Club;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    [Name("Club")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Club : InteractiveBase
    {
        private readonly ClubService _club;

        public Club(ClubService club) => _club = club;

        [Name("Club List")]
        [Command("club list", "clubs")]
        [Description("Paginates all clubs")]
        [Remarks("club list")]
        [RequiredChannel]
        public async Task ClubListAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubInfos.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (clubs.Count == 0)
                {
                    await Context.ReplyAsync("No clubs on this server");
                    return;
                }

                var pages = new List<string>();
                for (var i = 0; i < clubs.Count; i++)
                {
                    var x = clubs[i];
                    if (x.LeaderId == 1) continue;
                    var memberCount =
                        await db.ClubPlayers.CountAsync(y => y.GuildId == Context.Guild.Id && y.ClubId == x.Id);
                    if (memberCount == 0) continue;
                    var leader = Context.Guild.GetUser(x.LeaderId).Mention ??
                                 "Couldn't find user or left server.";
                    pages.Add($"**{x.Name} (id: {x.Id})**\n" +
                              $"Members: {memberCount}\n" +
                              $"Leader {leader}\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild,
                    $"Clubs in {Context.Guild.Name}", null));
            }
        }

        [Name("Club Check")]
        [Command("club check")]
        [Description("Checks specific club information")]
        [Remarks("club check 15")]
        [RequiredChannel]
        public async Task ClubCheckAsync(int id)
        {
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("Couldn't find a club with that ID.", Color.Red.RawValue);
                    return;
                }

                var clubUsers =
                    await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.ClubId == club.Id).ToListAsync();
                var officers = new StringBuilder();
                foreach (var x in clubUsers.Where(x => x.Rank == 2))
                {
                    officers.AppendLine($"{Context.Guild.GetUser(x.UserId).Mention}\n");
                }

                if (officers.Length == 0) officers.AppendLine("No officers");

                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = club.ImageUrl,
                    Timestamp = club.CreationDate,
                    Author = new EmbedAuthorBuilder { IconUrl = club.IconUrl, Name = $"{club.Name} (ID:{club.Id})" },
                    Footer = new EmbedFooterBuilder { Text = "Created:" },
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder { IsInline = false, Name = "Leader", Value = $"{Context.Guild.GetUser(club.LeaderId).Mention ?? "Couldn't find user or left server."}"},
                        new EmbedFieldBuilder { IsInline = false, Name = "Officers", Value = officers}
                    }
                }.CreateDefault(club.Description, Context.Guild.Id);
                await Context.ReplyAsync(embed);
            }
        }
    }
}