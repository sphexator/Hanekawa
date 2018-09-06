using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Hanekawa.Services.Level;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Account
{
    [Group("level")]
    public class LevelAdmin : InteractiveBase
    {
        private readonly LevelingService _levelingService;
        public LevelAdmin(LevelingService levelingService)
        {
            _levelingService = levelingService;
        }

        [Command("role stack", RunMode = RunMode.Async)]
        [Alias("stack", "rolestack", "rs")]
        [Summary("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task LevelStack()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.StackLvlRoles)
                {
                    cfg.StackLvlRoles = false;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Users will now only keep the highest earned role.",
                            Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.StackLvlRoles = true;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Level roles does now stack.",
                            Color.Green.RawValue).Build());
                }
            }
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
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
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
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
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
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
                                         "\n";
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
                        Title = $"Level roles for {Context.Guild.Name}",
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

        [Command("exp", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Starts a exp event with specified multiplier and duration. Auto-announced in Event channel if desired")]
        public async Task ExpEventAsync(uint multiplier, TimeSpan? duration = null)
        {
            try
            {
                if (!duration.HasValue) duration = TimeSpan.FromDays(1);
                if (duration.Value > TimeSpan.FromDays(1)) duration = TimeSpan.FromDays(1);
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply(
                        $"Wanna activate a exp event with multiplier of {multiplier} for {duration.Value.Humanize()} ({duration.Value.Humanize()}) ? (y/n)",
                        Color.Purple.RawValue).Build());
                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (response.Content.ToLower() != "y") return;

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Do you want to announce the event? (y/n)",
                        Color.Purple.RawValue).Build());
                var announceResp = await NextMessageAsync(true, true, TimeSpan.FromSeconds(60));
                if (announceResp.Content.ToLower() == "y")
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Okay, I'll let you announce it.",
                            Color.Green.RawValue).Build());
                    await _levelingService.AddExpMultiplierAsync(Context.Guild, multiplier, duration.Value);
                }
                else
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Announcing event into designated channel.",
                            Color.Green.RawValue).Build());
                    await _levelingService.AddExpMultiplierAsync(Context.Guild, multiplier, duration.Value, true, Context.Channel as SocketTextChannel);
                }
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Exp event setup aborted.",
                        Color.Red.RawValue).Build());
            }
        }
    }
}