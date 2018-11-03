using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Account.Achievement
{
    public class Achievement : InteractiveBase
    {
        [Command("achievement", RunMode = RunMode.Async)]
        [Alias("achiev")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievementLog([Remainder] string tab = null)
        {
            var user = Context.User as IGuildUser;
            using (var db = new DbService())
            {
                if (tab.IsNullOrWhiteSpace())
                {
                    var tabs = await db.AchievementTypes.ToListAsync();
                    string content = null;
                    foreach (var x in tabs) content += $"{x.Name}\n";
                    var author = new EmbedAuthorBuilder
                    {
                        Name = "Achievement tabs"
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = "Use `Achievement <tab>` to see list of achievements in that tab"
                    };
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Purple,
                        Description = content,
                        Author = author,
                        Footer = footer
                    };
                    await ReplyAsync(null, false, embed.Build());
                }
                else
                {
                    var type = await db.AchievementTypes.FirstOrDefaultAsync(x => x.Name == tab);
                    if (type == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("No tabs with that name", Color.Red.RawValue).Build());
                        return;
                    }

                    var achievements = await db.Achievements.Where(x => !x.Hidden && x.TypeId == type.TypeId)
                        .ToListAsync();
                    var pages = new List<string>();
                    for (var i = 0; i < achievements.Count;)
                    {
                        string achievString = null;
                        for (var j = 0; j < 5; j++)
                            try
                            {
                                if (i == achievements.Count) continue;
                                var Achiev = achievements[i];
                                achievString +=
                                    $"{Achiev.Name}({Achiev.AchievementId}) - Req: {Achiev.Requirement}\n";
                                i++;
                            }
                            catch
                            {
                                i++;
                            }

                        pages.Add(achievString);
                    }

                    var paginator = new PaginatedMessage
                    {
                        Color = Color.Purple,
                        Pages = pages,
                        Title = $"Achievements in {type.Name}",
                        Options = new PaginatedAppearanceOptions
                        {
                            First = new Emoji("⏮"),
                            Back = new Emoji("◀"),
                            Next = new Emoji("▶"),
                            Last = new Emoji("⏭"),
                            Stop = null,
                            Jump = null,
                            Info = null
                        }
                    };
                    await PagedReplyAsync(paginator);
                }
            }
        }

        [Command("achievement inspect", RunMode = RunMode.Async)]
        [Alias("ainspect", "achiv inspect", "inspect achieve")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievementInspect(int id)
        {
            using (var db = new DbService())
            {
                var achiv = await db.Achievements.FindAsync(id);
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Title = achiv.Name,
                    Description = $"{achiv.Description}\nRequired: {achiv.Requirement}",
                    ThumbnailUrl = achiv.ImageUrl
                };
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("achieved")]
        [RequiredChannel]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AchievedList()
        {
            using (var db = new DbService())
            {
                var unlock = await db.AchievementUnlocks.Where(x => x.UserId == Context.User.Id).ToListAsync();
                if (unlock.Count == 0)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("No achievements found.", Color.Red.RawValue).Build());
                    return;
                }

                var achievements = new List<AchievementMeta>();
                unlock.ForEach(x => achievements.Add(db.Achievements.Find(x.AchievementId)));
                var pages = new List<string>();
                for (var i = 0; i < achievements.Count;)
                {
                    string achievString = null;
                    for (var j = 0; j < 5; j++)
                        try
                        {
                            if (i == achievements.Count) continue;
                            var Achiev = achievements[i];
                            achievString +=
                                $"{Achiev.Name}({Achiev.AchievementId}) - Req: {Achiev.Requirement}\n";
                            i++;
                        }
                        catch
                        {
                            i++;
                        }

                    pages.Add(achievString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Unlocked Achievements for {Context.User.Username}",
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
            }
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