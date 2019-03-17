using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;

namespace Hanekawa.Modules.Permission
{
    [Name("log")]
    [Summary("Manage logging settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class LogPermission : InteractiveBase
    {
        [Name("Warn logging")]
        [Command("log warn", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nEnable/disable warn logging, leave empty to disable")]
        [Remarks("h.log warn #general")]
        public async Task LogWarnAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogWarn = null;
                    await Context.ReplyAsync("Disabled logging of warnings!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogWarn = channel.Id;
                await Context.ReplyAsync($"Set warn logging channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Join/Leave logging")]
        [Command("log join", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nEnable/disable join/leaves logging, leave empty to disable")]
        [Remarks("h.log join #general")]
        public async Task LogJoinAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogJoin = null;
                    await Context.ReplyAsync("Disabled logging of join/leave!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogJoin = channel.Id;
                await Context.ReplyAsync($"Set join/leave logging channel to {channel.Mention}!",
                    Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Message logging")]
        [Command("log message", RunMode = RunMode.Async)]
        [Alias("msg")]
        [Summary("**Require Manage Server**\nEnable/Disable message logging, leave empty to disable")]
        [Remarks("h.log msg #general")]
        public async Task LogMessageAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogMsg = null;
                    await Context.ReplyAsync("Disabled logging of messages!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogMsg = channel.Id;
                await Context.ReplyAsync($"Set message logging channel to {channel.Mention}!",
                    Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Ban logging")]
        [Command("log ban", RunMode = RunMode.Async)]
        [Alias("ban")]
        [Summary("**Require Manage Server**\nEnable/Disable moderation logging, leave empty to disable")]
        [Remarks("h.log ban #general")]
        public async Task LogBanAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogBan = null;
                    await Context.ReplyAsync("Disabled logging of moderation actions!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogBan = channel.Id;
                await Context.ReplyAsync($"Set mod log channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Automod logging")]
        [Command("log automod", RunMode = RunMode.Async)]
        [Summary("**Require Manage Server**\nEnable/Disable separate logging of auto-moderator")]
        [Remarks("h.log automod #general")]
        public async Task LogAutoModAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAutoMod = null;
                    await Context.ReplyAsync("Disabled separate logging of auto-moderator actions!",
                        Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAutoMod = channel.Id;
                await Context.ReplyAsync($"Set auto mod log channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("User logging")]
        [Command("log user")]
        [Summary("**Require Manage Server**\nEnable/Disable logging of user changes (avatar and name/nickname)")]
        [Remarks("h.log user #general")]
        public async Task LogUserUpdates(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAvi = null;
                    await Context.ReplyAsync("Disabled logging of user updates!",
                        Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAvi = channel.Id;
                await Context.ReplyAsync($"Set user update log channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }
    }
}