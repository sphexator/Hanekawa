﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Level
{
    [Name("Level")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public partial class Level : HanekawaCommandModule
    {
        private readonly ExpService _exp;
        public Level(ExpService exp) => _exp = exp;

        [Name("Level Reset")]
        [Command("lr", "lvlreset")]
        [Description("Reset the server level/exp back to 0")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.User)]
        public async Task ResetAsync()
        {
            await Context.ReplyAsync(
                "You sure you want to completely reset server levels/exp on this server?(y/n) \nthis change can't be reversed.");
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(
                x => x.Message.Author.Id.RawValue == Context.Member.Id.RawValue && x.Message.Guild.Id.RawValue == Context.Guild.Id.RawValue,
                TimeSpan.FromMinutes(1));
            if (response == null || response.Message.Content.ToLower() != "y")
            {
                await Context.ReplyAsync("Aborting...");
                return;
            }

            var msg = await Context.ReplyAsync("Server level reset in progress...");
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var users = db.Accounts.Where(x => x.GuildId == Context.Guild.Id.RawValue);
            foreach (var x in users)
            {
                x.Level = 1;
                x.Exp = 0;
                x.TotalExp = 0;
            }
            
            await db.SaveChangesAsync();
            try
            {
                var updEmbed = msg.Embeds.First().ToEmbedBuilder();
                updEmbed.Color = Color.Green;
                updEmbed.Description = "Server level reset complete.";
                await msg.ModifyAsync(x => x.Embed = updEmbed.Build());
            }
            catch
            {
                await Context.ReplyAsync("Server level reset complete!", Color.Green);
            }
        }

        [Name("Set Level")]
        [Command("sl", "setlvl")]
        [Description("Sets a user to a desired level")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetLevelAsync(CachedMember user, int level)
        {
            if (level <= 0) return;
            var totalExp = 0;
            for (var i = 1; i < level + 1; i++) totalExp += _exp.ExpToNextLevel(i);

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userdata = await db.GetOrCreateUserData(user);
            userdata.Level = level;
            userdata.Exp = 0;
            userdata.TotalExp = totalExp;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set {user.Mention} level to {level}", Color.Green);
        }

        [Name("Decay")]
        [Command("decay")]
        [Description("Toggles artificial level decay (decay starts after 14 days) No exp is lost")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task DecayAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
            if (cfg.Decay)
            {
                cfg.Decay = false;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    "Level decay has been disabled.",
                    Color.Green);
            }
            else
            {
                cfg.Decay = true;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Level decay has been activated. Decay starts after 14 days of user being inactive (hasn't sent a message).\n" +
                                         "No exp lost and exp returned to normal after sending a message again.",
                    Color.Green);
            }
        }

        [Name("Level Role Stack")]
        [Command("lrs", "lvlstack")]
        [Description("Toggles between level roles stacking or keep the highest earned one")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task StackToggleAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
            if (cfg.StackLvlRoles)
            {
                cfg.StackLvlRoles = false;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Users will now only keep the highest earned role.",
                    Color.Green);
            }
            else
            {
                cfg.StackLvlRoles = true;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Level roles does now stack.",
                    Color.Green);
            }
        }

        [Name("Level Stack Role Add")]
        [Command("lsa", "lvlsadd")]
        [Description("Adds a role reward which will stack regardless of setting (useful for permission role)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task StackAddAsync(int level, [Remainder] CachedRole role) =>
            await AddLevelRole(Context, level, role, true);

        [Name("Level Role Add")]
        [Command("la", "lvladd")]
        [Description("Adds a role reward")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddAsync(int level, [Remainder] CachedRole role) =>
            await AddLevelRole(Context, level, role, false);

        [Name("Level Role Remove")]
        [Command("lr", "lvlremove")]
        [Description("Removes a role reward")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveAsync(int level)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var role = await db.LevelRewards.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.Level == level);
            if (role == null)
            {
                await Context.ReplyAsync("Couldn't find a role with that level", Color.Red);
                return;
            }

            db.LevelRewards.Remove(role);
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Removed {Context.Guild.Roles.First(x => x.Key == role.Role).Value.Name} from level rewards!",
                Color.Green);
        }

        [Name("Level List")]
        [Command("lvlist")]
        [Description("Lists all role rewards")]
        [RequiredChannel]
        public async Task LevelListAsync()
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var levels = await db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id.RawValue).OrderBy(x => x.Level)
                .ToListAsync();
            if (levels == null || levels.Count == 0)
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
                    var role = Context.Guild.GetRole(x.Role);
                    if (role == null) pages.Add("Role not found");
                    else
                        pages.Add($"Name: {role.Name ?? "Role not found"}\n" +
                                  $"Level: {x.Level}\n" +
                                  $"Stack: {x.Stackable}");
                }
                catch
                {
                    pages.Add("Role not found");
                    //todo: Handle this better in the future
                }
            }

            await Context.PaginatedReply(pages, Context.Guild, $"Level Roles for {Context.Guild.Name}");
        }

        private async Task AddLevelRole(DiscordCommandContext context, int level, CachedRole role, bool stack)
        {
            if (level <= 0) return;
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.LevelRewards.FindAsync(context.Guild.Id.RawValue, level);
            if (check != null)
            {
                var gRole = context.Guild.GetRole(check.Role);
                if (gRole != null)
                {
                    await Context.ReplyAsync($"Do you wish to replace {gRole.Name} for level {check.Level}? (y/n)");
                    var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(
                        x => x.Message.Author.Id.RawValue == Context.Member.Id.RawValue && x.Message.Guild.Id.RawValue == Context.Guild.Id.RawValue,
                        TimeSpan.FromMinutes(1));
                    if (response == null || response.Message.Content.ToLower() != "y")
                    {
                        await Context.ReplyAsync("Cancelling.");
                        return;
                    }

                    if (response.Message.Content.ToLower() != "yes")
                    {
                        await Context.ReplyAsync("Cancelling.");
                        return;
                    }
                }
            }

            var data = new LevelReward
            {
                GuildId = context.Guild.Id.RawValue,
                Level = level,
                Role = role.Id.RawValue,
                Stackable = stack
            };
            await db.LevelRewards.AddAsync(data);
            await db.SaveChangesAsync();
            var users = await db.Accounts.Where(x => x.GuildId == role.Guild.Id.RawValue && x.Level >= level && x.Active)
                .ToListAsync();
            if (users.Count <= 10)
            {
                for (var i = 0; i < users.Count; i++)
                {
                    var x = users[i];
                    var user = await role.Guild.GetOrFetchMemberAsync(x.UserId);
                    if(user == null) continue;
                    await _exp.NewLevelManagerAsync(users[i], user as CachedMember, db);
                }
            }

            if (users.Count > 10) await Context.ReplyAsync($"Added {role.Name} as a lvl{level} reward!", Color.Green);
            else
                await Context.ReplyAsync(
                    $"Added {role.Name} as a lvl{level} reward, and applied to {users.Count} users", Color.Green);
        }
    }
}