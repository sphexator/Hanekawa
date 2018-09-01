using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Account
{
    public partial class Account
    {
        [Group("level")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public class LevelAdmin : InteractiveBase
        {
            [Command("add", RunMode = RunMode.Async)]
            [Summary("Adds a role reward")]
            public async Task LevelAdd(uint level, [Remainder]string roleName)
            {
                if (roleName.IsNullOrWhiteSpace()) return;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Couldn't find a role by that name.", Color.Red.RawValue).Build());
                    return;
                }

                using (var db = new DbService())
                {
                    var check = await db.LevelRewards.FindAsync(Context.Guild.Id, level);
                    if (check != null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"There's already a level reward on that level.", Color.Red.RawValue).Build());
                        return;
                    }

                    var data = new LevelReward
                    {
                        GuildId = Context.Guild.Id,
                        Level = level,
                        Role = role.Id,
                        Stackable = false
                    };
                    await db.LevelRewards.AddAsync(data);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Added {role.Name} as a lvl{level} reward!", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("create", RunMode = RunMode.Async)]
            [Summary("Creates a role reward with given level and name")]
            public async Task LevelCreate(uint level, [Remainder]string roleName)
            {
                if (roleName.IsNullOrWhiteSpace()) return;
                using (var db = new DbService())
                {
                    var role = await Context.Guild.CreateRoleAsync(roleName, GuildPermissions.None);
                    var data = new LevelReward
                    {
                        GuildId = Context.Guild.Id,
                        Level = level,
                        Role = role.Id,
                        Stackable = false
                    };
                    await db.LevelRewards.AddAsync(data);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Successfully created and added {role.Name} as a level reward for level{level}!",
                                Color.Green.RawValue).Build());
                }
            }

            [Command("remove", RunMode = RunMode.Async)]
            [Summary("Removes a role reward with given level")]
            public async Task LevelRemove(uint level)
            {
                using (var db = new DbService())
                {
                    var check = await db.LevelRewards.FindAsync(Context.Guild.Id, level);
                    if (check == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"There is no reward connected to that level.", Color.Red.RawValue)
                                .Build());
                        return;
                    }
                    var role = db.LevelRewards.First(x => x.GuildId == Context.Guild.Id && x.Level == level);
                    db.LevelRewards.Remove(role);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Removed {Context.Guild.Roles.First(x => x.Id == role.Role).Name} from level rewards!", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("list", RunMode = RunMode.Async)]
            [Summary("Lists all role rewards")]
            public async Task LevelAdd()
            {
                using (var db = new DbService())
                {
                    var list = await db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.Level)
                        .ToListAsync();
                    var pages = new List<string>();
                    if (pages.Count != 0)
                    {
                        for (var i = 0; i < pages.Count;)
                        {
                            try
                            {
                                string input = null;
                                for (var j = 0; j < 5; j++)
                                {
                                    if (i >= pages.Count) continue;
                                    var entry = list[i];
                                    input += $"Name: {Context.Guild.GetRole(entry.Role).Name ?? "Role not found"}\n" +
                                             $"Level: {entry.Level}\n" +
                                             $"Stack: {entry.Stackable}\n" +
                                             $"\n";
                                    pages.Add(input);
                                    i++;
                                }

                                pages.Add(input);
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        var paginator = new PaginatedMessage
                        {
                            Color = Color.Purple,
                            Pages = pages,
                            Title = $"Welcome banners for {Context.Guild.Name}",
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
                    else await ReplyAsync(null, false, new EmbedBuilder().Reply("No level roles added!").Build());
                }
            }
        }
    }
}