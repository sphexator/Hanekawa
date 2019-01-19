using System;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Level;
using Hanekawa.Services.Level.Util;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Modules.Account.Level
{
    [Group("level")]
    public class LevelAdmin : InteractiveBase
    {
        private readonly LevelingService _levelingService;
        private readonly Calculate _calculate;
        private readonly LogService _log;
        public LevelAdmin(LevelingService levelingService, Calculate calculate, LogService log)
        {
            _levelingService = levelingService;
            _calculate = calculate;
            _log = log;
        }

        [Command("setlevel")]
        [Summary("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLevel(SocketGuildUser user, int level)
        {
            if (level <= 0) return;
            var totalExp = 0;
            for (var i = 1; i < level + 1; i++)
            {
                totalExp += _calculate.GetServerLevelRequirement(i);
            }

            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                userdata.Level = level;
                userdata.Exp = 0;
                userdata.TotalExp = totalExp;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set {user.Mention} level to {level}", Color.Green.RawValue);
            }
        }

        [Command("role stack", RunMode = RunMode.Async)]
        [Alias("stack", "rolestack", "rs")]
        [Summary("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task LevelStack()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (cfg.StackLvlRoles)
                {
                    cfg.StackLvlRoles = false;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Users will now only keep the highest earned role.",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.StackLvlRoles = true;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Level roles does now stack.",
                        Color.Green.RawValue);
                }
            }
        }

        [Command("stack add", RunMode = RunMode.Async)]
        [Alias("sadd")]
        [Summary("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task ExclusiveAdd(int level, [Remainder] IRole role) =>
            await AddLevelRole(Context, level, role, true);

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task LevelAdd(int level, [Remainder] IRole role) =>
            await AddLevelRole(Context, level, role, false);

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Creates a role reward with given level and name")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task LevelCreate(int level, [Remainder] string roleName)
        {
            if (level <= 0) return;
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
                await Context.ReplyAsync($"Successfully created and added {role.Name} as a level reward for level{level}!",
                    Color.Green.RawValue);
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
                var role = await db.LevelRewards.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.Level == level);
                if (role == null)
                {
                    await Context.ReplyAsync("Couldn't find a role with that level", Color.Red.RawValue);
                    return;
                }
                db.LevelRewards.Remove(role);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Removed {Context.Guild.Roles.First(x => x.Id == role.Role).Name} from level rewards!",
                    Color.Green.RawValue);
            }
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all role rewards")]
        [RequiredChannel]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LevelAdd()
        {
            using (var db = new DbService())
            {
                var levels = await db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.Level)
                    .ToListAsync();
                if (levels.Count == 0)
                {
                    await Context.ReplyAsync("No level roles added.");
                    return;
                }
                var pages = new List<string>();
                foreach (var x in levels)
                {
                    try
                    {
                        var role = Context.Guild.GetRole(x.Role) ?? Context.Guild.Roles.FirstOrDefault(z => z.Id == x.Role);
                        if(role == null) pages.Add("Role not found");
                        else
                        {
                            pages.Add($"Name: {role.Name ?? "Role not found"}\n" +
                                      $"Level: {x.Level}\n" +
                                      $"Stack: {x.Stackable}\n" +
                                      "\n");
                        }
                    }
                    catch (Exception e)
                    {
                        pages.Add("Role not found\n");
                        _log.LogAction(LogLevel.Error, e.ToString(), "LevelAdmin");
                    }
                }
                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild, $"Level roles for {Context.Guild.Name}"));
            }
        }

        private async Task AddLevelRole(SocketCommandContext context, int level, IRole role, bool stack)
        {
            if (level <= 0) return;
            using (var db = new DbService())
            {
                var check = await db.LevelRewards.FindAsync(context.Guild.Id, level);
                if (check != null)
                {
                    var gRole = context.Guild.GetRole(check.Role);
                    if (gRole != null)
                    {
                        await context.ReplyAsync($"Do you wish to replace {gRole.Name} for level {check.Level}? (y/n)");
                        var response = await NextMessageAsync();
                        if (response.Content.ToLower() != "y" || response.Content.ToLower() != "yes")
                        {
                            await context.ReplyAsync("Cancelling.");
                            return;
                        }
                    }
                }

                var data = new LevelReward
                {
                    GuildId = context.Guild.Id,
                    Level = level,
                    Role = role.Id,
                    Stackable = stack
                };
                await db.LevelRewards.AddAsync(data);
                await db.SaveChangesAsync();
                await context.ReplyAsync($"Added {role.Name} as a lvl{level} reward!", Color.Green.RawValue);
            }
        }
    }
}