using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Achievement
{
    public class Achievement : InteractiveBase
    {
        private readonly DbService _db;

        public Achievement(DbService db)
        {
            _db = db;
        }

        [Command("achievement", RunMode = RunMode.Async)]
        [Alias("achieve", "achiev")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievementLog([Remainder] string tab = null)
        {
            if (tab.IsNullOrWhiteSpace())
            {
                var tabs = await _db.AchievementTypes.ToListAsync();
                string content = null;
                foreach (var x in tabs) content += $"{x.Name}\n";
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder {Name = "Achievement tabs"},
                    Color = Color.Purple,
                    Description = content,
                    Footer = new EmbedFooterBuilder
                        {Text = "Use `Achievement <tab>` to see list of achievements in that tab"}
                };
                await Context.ReplyAsync(embed);
            }
            else
            {
                var type = await _db.AchievementTypes.FirstOrDefaultAsync(x => x.Name == tab);
                if (type == null)
                {
                    await Context.ReplyAsync("No tabs with that name", Color.Red.RawValue);
                    return;
                }

                var achievements = await _db.Achievements.Where(x => !x.Hidden && x.TypeId == type.TypeId)
                    .ToListAsync();
                var pages = new List<string>();
                foreach (var x in achievements) pages.Add($"{x.Name} - Req: {x.Requirement}\n");

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Achievements in {type.Name}"));
            }
        }

        [Command("achievement inspect", RunMode = RunMode.Async)]
        [Alias("ainspect", "achiv inspect", "inspect achieve")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievementInspect(int id)
        {
            var achiv = await _db.Achievements.FindAsync(id);
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
                Title = achiv.Name,
                Description = $"{achiv.Description}\nRequired: {achiv.Requirement}",
                ThumbnailUrl = achiv.ImageUrl
            };
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("achieved")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievedList()
        {
            var unlock = await _db.AchievementUnlocks.Where(x => x.UserId == Context.User.Id).ToListAsync();
            if (unlock.Count == 0)
            {
                await Context.ReplyAsync("No achievements found", Color.Red.RawValue);
                return;
            }

            var achievements = new List<AchievementMeta>();
            unlock.ForEach(x => achievements.Add(_db.Achievements.Find(x.AchievementId)));
            var pages = new List<string>();
            foreach (var x in achievements)
                pages.Add($"{x.Name}({x.AchievementId}) - Req: {x.Requirement}\n");

            await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Unlocked Achievements for {Context.User.Username}"));
        }

        private static string AchievType(IEnumerable<AchievementUnlock> list,
            List<AchievementMeta> achievements, AchievementUnlock achiev = null)
        {
            if (achiev == null) return "Server | Global";
            if (ServerAndGlobal(list, achievements, achiev)) return "**Server | Global**";
            return achiev.Achievement.Global ? "Server | **Global**" : "**Server** | Global";
        }

        private static bool ServerAndGlobal(IEnumerable<AchievementUnlock> list, List<AchievementMeta> achievements,
            AchievementUnlock achiev = null)
        {
            if (achiev == null) return false;
            if (!achiev.Achievement.Global) return false;
            var result = list.Where(
                x => x.TypeId == achiev.TypeId && x.Achievement.Requirement == achiev.Achievement.Requirement);
            return result.Count() == 2;
        }
    }
}