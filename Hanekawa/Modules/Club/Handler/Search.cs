using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Club.Handler
{
    public class Search : InteractiveBase, IHanaService
    {
        public async Task Check(ICommandContext context, int id)
        {
            using (var db = new DbService())
            {
                var club = await db.GetClubAsync(id, context.Guild);
                if (club == null)
                {
                    await context.ReplyAsync("Couldn't find a club with that ID.", Color.Red.RawValue);
                    return;
                }

                await context.ReplyAsync($"**{club.Name} (ID:{club.Id}**\n" +
                                         $"Members: {await db.ClubPlayers.CountAsync(x => x.GuildId == context.Guild.Id && x.ClubId == club.Id)}\n" +
                                         $"Leader {(await context.Guild.GetUserAsync(club.LeaderId)).Mention ?? "Couldn't find user or left server."}");
            }
        }

        public async Task ClubListAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubInfos.Where(x => x.GuildId == context.Guild.Id).ToListAsync();
                if (clubs.Count == 0)
                {
                    await context.ReplyAsync("No clubs on this server");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in clubs)
                {
                    var leader = (await context.Guild.GetUserAsync(x.LeaderId)).Mention ??
                                 "Couldn't find user or left server.";
                    pages.Add($"**{x.Name} (id: {x.Id})**\n" +
                              $"Members: {await db.ClubPlayers.CountAsync(y => y.GuildId == context.Guild.Id && y.ClubId == x.Id)}\n" +
                              $"Leader {leader}\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(context.Guild.Id, context.Guild,
                    $"Clubs in {context.Guild.Name}"));
            }
        }
    }
}
