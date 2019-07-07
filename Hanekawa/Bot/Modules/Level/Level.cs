using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Cooldown = Hanekawa.Shared.Command.Cooldown;

namespace Hanekawa.Bot.Modules.Level
{
    [Name("Level")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Level : InteractiveBase
    {
        private readonly ExpService _exp;
        public Level(ExpService exp) => _exp = exp;

        [Name("Level Reset")]
        [Command("lr", "lvlreset")]
        [Description("Reset the server level/exp back to 0")]
        [RequireServerOwner]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.WhateverWithMoreSalt)]
        public async Task ResetAsync()
        {
            await Context.ReplyAsync("You sure you want to completely reset server levels/exp on this server?(y/n) \nthis change can't be reversed.");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            if (response == null || response.Content.ToLower() != "yes" || response.Content.ToLower() != "y")
            {
                await Context.ReplyAsync("Aborting...");
                return;
            }

            var msg = await Context.ReplyAsync("Server level reset in progress...");
            using (var db = new DbService())
            {
                var users = db.Accounts.Where(x => x.GuildId == Context.Guild.Id);
                foreach (var x in users)
                {
                    x.Level = 1;
                    x.Exp = 0;
                    x.TotalExp = 0;
                }

                db.Accounts.UpdateRange(users);
                await db.SaveChangesAsync();
            }

            var updEmbed = msg.Embeds.First().ToEmbedBuilder();
            updEmbed.Color = Color.Green;
            updEmbed.Description = "Server level reset complete.";
            await msg.ModifyAsync(x => x.Embed = updEmbed.Build());
        }

        [Name("Set Level")]
        [Command("sl", "setlvl")]
        [Description("Sets a user to a desired level")]
        [RequireServerOwner]
        public async Task SetLevelAsync(SocketGuildUser user, int level)
        {
            if (level <= 0) return;
            var totalExp = 0;
            for (var i = 1; i < level + 1; i++) totalExp += _exp.ExpToNextLevel(i);

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

        [Name("Level Role Stack")]
        [Command("lrs", "lvlstack")]
        [Description("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task StackToggleAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
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

        [Name("Level Stack Role Add")]
        [Command("lsa", "lvlsadd")]
        [Description("Adds a role reward which will stack regardless of setting (useful for permission role)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task StackAddAsync(int level, [Remainder]SocketRole role) =>
            await AddLevelRole(Context, level, role, true);

        [Name("Level Role Add")]
        [Command("la", "lvladd")]
        [Description("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddAsync(int level, [Remainder] SocketRole role) =>
            await AddLevelRole(Context, level, role, false);

        [Name("Level Role Remove")]
        [Command("lr", "lvlremove")]
        [Description("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveAsync(int level)
        {
            using (var db = new DbService())
            {
                var role = await db.LevelRewards.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.Level == level);
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

        [Name("Level List")]
        [Command("ll", "lvllist")]
        [Description("Lists all role rewards")]
        [RequiredChannel]
        public async Task LevelListAsync()
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
                for (var i = 0; i < levels.Count; i++)
                {
                    var x = levels[i];
                    try
                    {
                        var role = Context.Guild.GetRole(x.Role) ??
                                   Context.Guild.Roles.FirstOrDefault(z => z.Id == x.Role);
                        if (role == null) pages.Add("Role not found");
                        else
                            pages.Add($"Name: {role.Name ?? "Role not found"}\n" +
                                      $"Level: {x.Level}\n" +
                                      $"Stack: {x.Stackable}\n" +
                                      "\n");
                    }
                    catch
                    {
                        pages.Add("Role not found\n");
                        //todo: Handle this better in the future
                    }
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Level Roles for {Context.Guild.Name}",
                    null));
            }
        }

        private async Task AddLevelRole(HanekawaContext context, int level, SocketRole role, bool stack)
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