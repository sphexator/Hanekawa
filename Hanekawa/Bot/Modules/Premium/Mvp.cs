using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Mvp;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Premium
{
    [Name("MVP")]
    [Description("Premium Module. Assign most active users of the week a role.")]
    [RequirePremium]
    public class Mvp : HanekawaCommandModule
    {
        [Name("Set Channel")]
        [Command("mvpchannel", "mvpc")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetAnnouncementChannelAsync(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.MvpChannel = null;
                await Context.ReplyAsync("Disabled MVP Announcement!");
            }
            else
            {
                cfg.MvpChannel = channel.Id.RawValue;
                await Context.ReplyAsync($"Set MVP Announcement channel to {channel.Mention} !");
            }

            await db.SaveChangesAsync();
        }

        [Name("Set Role")]
        [Command("mvprole", "mvpr")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetMvpRoleAsync(CachedRole role = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateMvpConfigAsync(Context.Guild);
            if (role == null)
            {
                cfg.RoleId = null;
                await Context.ReplyAsync("Disabled MVP!");
            }
            else
            {
                cfg.RoleId = role.Id.RawValue;
                await Context.ReplyAsync($"Set MVP role to {role.Name} !");
            }

            await db.SaveChangesAsync();
        }

        [Name("MVP Reward Day")]
        [Command("mvpday")]
        [Description("Sets which day the MVP rewards is gonna be handed out")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetMvpDayAsync(DayOfWeek? day = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateMvpConfigAsync(Context.Guild);
            if (!day.HasValue)
            {
                cfg.Disabled = true;
                await db.SaveChangesAsync();
                await ReplyAsync("Disabled MVP service!", Color.Red);
                return;
            }

            if (day != cfg.Day)
            {
                cfg.Disabled = false;
                cfg.Day = day.Value;
                await db.SaveChangesAsync();
                await ReplyAsync($"Set MVP reward day to {day.Value}!", Color.Green);
                return;
            }

            await ReplyAsync($"Mvp reward is already set to {day.Value}!");
        }

        [Name("Force Mvp Reward")]
        [Command("mvpforce")]
        [Description("Force MVP rewards incase it doesn't go out")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task ForceMvpAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var guildConfig = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            await ReplyAsync("Executing MVP rewards...");
            await Context.ServiceProvider.GetRequiredService<MvpService>().Reward(guildConfig, db, true);
            await ReplyAsync("Rewarded MVP users and reset MVP counter");
        }

        [Name("Opt Out")]
        [Command("Optout")]
        [RequiredChannel]
        public async Task OptOutAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Member);
            if (userData.MvpOptOut)
            {
                userData.MvpOptOut = false;
                await Context.ReplyAsync("Re-enabled MVP counting!");
            }
            else
            {
                userData.MvpOptOut = true;
                await Context.ReplyAsync("Opted out of MVP counting!");
            }

            await db.SaveChangesAsync();
        }
    }
}
