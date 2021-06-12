using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Level")]
    [Description("Commands for levels")]
    public class Level : HanekawaCommandModule
    {
        [Name("Level List")]
        [Command("lvlist")]
        [Description("Lists all role rewards")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> LevelListAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var levels = await db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id.RawValue).OrderBy(x => x.Level)
                .ToListAsync();
            if (levels == null || levels.Count == 0) return Reply("No level roles added.");

            var pages = new List<string>();
            foreach (var x in levels)
            {
                try
                {
                    pages.Add(!Context.Guild.Roles.TryGetValue(x.Role, out var role)
                        ? "Role not found"
                        : $"Name: {role.Name ?? "Role not found"}\nLevel: {x.Level}\nStack: {x.Stackable}");
                }
                catch
                {
                    pages.Add("Role not found");
                    //todo: Handle this better in the future
                }
            }
            
            return Pages(pages.Pagination(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"Level Roles for {Context.Guild.Name}"));
        }
        
        [Name("Level Admin")]
        [Description("Commands to manage level module")]
        [Group("level")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public class LevelAdmin : Level
        {
            [Name("Level Reset")]
            [Command("reset")]
            [Description("Reset the server level/exp back to 0")]
            [RequireAuthorGuildPermissions(Permission.Administrator)]
            [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.Member)]
            public async Task<DiscordCommandResult> ResetAsync()
            {
                var cache = Context.Services.GetRequiredService<CacheService>();
                await Reply(
                    "You sure you want to completely reset server levels/exp on this server?(y/n) \nthis change can't be reversed.",
                    cache.GetColor(Context.GuildId));
                var response = await Context.WaitForMessageAsync(
                    x => x.Message.Author.Id == Context.Author.Id &&
                         x.Message.GuildId == Context.Guild.Id,
                    TimeSpan.FromMinutes(1));
                if (response == null || (response.Message.Content.ToLower() != "y" ||
                                         response.Message.Content.ToLower() != "yes")) return Reply("Aborting...");

                var msg = await Context.Channel.SendMessageAsync(new LocalMessage().Create(
                    "Server level reset in progress...",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId)));

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
                    var updEmbed = LocalEmbed.FromEmbed(msg.Embeds[0]);
                    updEmbed.Color = HanaBaseColor.Ok();
                    updEmbed.Description = "Server level reset complete.";
                    await msg.ModifyAsync(x => x.Embed = updEmbed);
                }
                catch
                {
                    return Reply("Server level reset complete!", HanaBaseColor.Ok());
                }

                return null;
            }

            [Name("Set Level")]
            [Command("set")]
            [Description("Sets a user to a desired level")]
            [RequireAuthorGuildPermissions(Permission.Administrator)]
            public async Task<DiscordCommandResult> SetLevelAsync(IMember user, int level)
            {
                if (level <= 0) return null;
                var exp = Context.Services.GetRequiredService<ExpService>();
                var totalExp = 0;
                for (var i = 1; i < level + 1; i++) totalExp += exp.ExpToNextLevel(i);

                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var userdata = await db.GetOrCreateUserData(user);
                userdata.Level = level;
                userdata.Exp = 0;
                userdata.TotalExp = totalExp;
                await db.SaveChangesAsync();
                return Reply($"Set {user.Mention} level to {level}", HanaBaseColor.Ok());
            }

            [Name("Decay")]
            [Command("decay")]
            [Description("Toggles artificial level decay (decay starts after 14 days) No exp is lost")]
            public async Task<DiscordCommandResult> DecayAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                if (cfg.Decay)
                {
                    cfg.Decay = false;
                    await db.SaveChangesAsync();
                    return Reply("Level decay has been disabled.", HanaBaseColor.Ok());
                }
                cfg.Decay = true;
                await db.SaveChangesAsync();
                return Reply(
                    "Level decay has been activated. Decay starts after 14 days of user being inactive (hasn't sent a message).\n" +
                    "No exp lost and exp returned to normal after sending a message again.",
                    HanaBaseColor.Ok());
            }

            [Name("Event")]
            [Command("event")]
            [Description(
                "Create a experience multiplier event. Increase the multiplier to 2x, 3x or anything for a specified duration")]
            public async Task<DiscordCommandResult> EventAsync(double multiplier, TimeSpan duration)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var exp = Context.Services.GetRequiredService<ExpService>();
                await exp.StartEventAsync(db, Context, multiplier, duration);
                return Reply($"A experience event of {multiplier}x has been initiated for {duration.Humanize()}", HanaBaseColor.Ok());
            }

            [Name("Multiplier Check")]
            [Command("multiplier", "multi")]
            [Description("Check the current multiplier for the various sources (text/voice/other)")]
            public async Task<DiscordResponseCommandResult> MultiCheckAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cache = Context.Services.GetRequiredService<CacheService>();
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.GuildId);
                cache.TryGetMultiplier(ExpSource.Text, Context.GuildId, out var textMulti);
                cache.TryGetMultiplier(ExpSource.Voice, Context.GuildId, out var voiceMulti);
                cache.TryGetMultiplier(ExpSource.Other, Context.GuildId, out var otherMulti);
                return Reply(new LocalEmbed
                {
                    Author = new LocalEmbedAuthor
                        {Name = $"{Context.Guild.Name} Experience Multipliers", IconUrl = Context.Guild.GetIconUrl()},
                    Color = cache.GetColor(Context.GuildId),
                    Description = $"Text: {textMulti}x (default: {cfg.TextExpMultiplier}x)\n" +
                                  $"Voice: {voiceMulti}x (default: {cfg.VoiceExpMultiplier}x)\n" +
                                  $"Other: {otherMulti}x (default: 1x)"
                });
            }
            
            [Name("Level Role Stack")]
            [Command("stack")]
            [Description("Toggles between level roles stacking or keep the highest earned one")]
            public async Task<DiscordCommandResult> StackToggleAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                if (cfg.StackLvlRoles)
                {
                    cfg.StackLvlRoles = false;
                    await db.SaveChangesAsync();
                    return Reply("Users will now only keep the highest earned role.", HanaBaseColor.Ok());
                }

                cfg.StackLvlRoles = true;
                await db.SaveChangesAsync();
                return Reply("Level roles does now stack.",
                    HanaBaseColor.Ok());
            }
            
            [Name("Level Role Add")]
            [Command("add")]
            [Description("Adds a role reward")]
            public async Task<DiscordCommandResult> AddAsync(int level, [Remainder] IRole role) =>
                await AddLevelRole(Context, level, role, false);

            [Name("Level Stack Role Add")]
            [Command("stackadd")]
            [Description("Adds a role reward which will stack regardless of setting (useful for permission role)")]
            public async Task<DiscordCommandResult> StackAddAsync(int level, [Remainder] IRole role) =>
                await AddLevelRole(Context, level, role, true);

            [Name("Level Role Remove")]
            [Command("remove")]
            [Description("Removes a role reward")]
            public async Task<DiscordCommandResult> RemoveAsync(int level)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var role = await db.LevelRewards.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id.RawValue && x.Level == level);
                if (role == null) return Reply("Couldn't find a role with that level", HanaBaseColor.Bad());

                db.LevelRewards.Remove(role);
                await db.SaveChangesAsync();
                return Reply(
                    $"Removed {Context.Guild.Roles.First(x => x.Key == role.Role).Value.Name} from level rewards!",
                    HanaBaseColor.Ok());
            }

            private async Task<DiscordCommandResult> AddLevelRole(HanekawaCommandContext context, int level,
                IRole role, bool stack)
            {
                if (level <= 0) return null;

                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var check = await db.LevelRewards.FindAsync(context.Guild.Id, level);
                if (check != null)
                {
                    if (context.Guild.Roles.TryGetValue(check.Role, out var gRole))
                    {
                        await Reply($"Do you wish to replace {gRole.Name} for level {check.Level}? (y/n)");
                        var response = await Context.WaitForMessageAsync(
                            x => x.Message.Author.Id == Context.Author.Id && x.Message.GuildId == Context.Guild.Id,
                            TimeSpan.FromMinutes(1));
                        if (response == null || (response.Message.Content.ToLower() != "y" ||
                                                 response.Message.Content.ToLower() != "yes"))
                            return Reply("Cancelling.", HanaBaseColor.Bad());
                    }
                }

                var data = new LevelReward
                {
                    GuildId = context.Guild.Id,
                    Level = level,
                    Role = role.Id.RawValue,
                    Stackable = stack
                };
                await db.LevelRewards.AddAsync(data);
                await db.SaveChangesAsync();
                var users = await db.Accounts.Where(x => x.GuildId == role.GuildId && x.Level >= level && x.Active)
                    .ToArrayAsync();
                if (users.Length <= 10)
                {
                    var exp = Context.Services.GetRequiredService<ExpService>();
                    foreach (var x in users)
                    {
                        var user = await Context.Guild.GetOrFetchMemberAsync(x.UserId);
                        if (user == null) continue;
                        await exp.LevelUpCheckAsync(user, x, db, x.Decay);
                    }
                }

                return Reply(
                    users.Length > 10
                        ? $"Added {role.Name} as a lvl{level} reward!"
                        : $"Added {role.Name} as a lvl{level} reward, and applied to {users.Length} users",
                    HanaBaseColor.Ok());
            }
        }
    }
}