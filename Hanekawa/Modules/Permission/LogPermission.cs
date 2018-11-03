using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Modules.Permission
{
    [Group("log")]
    [Summary("Manage logging settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class LogPermission : InteractiveBase
    {
        [Command("warn", RunMode = RunMode.Async)]
        [Summary("Enable/disable warn logging, leave empty to disable")]
        public async Task LogWarnAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.LogWarn = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled logging of warnings!", Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogWarn = channel.Id;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set warn logging channel to {channel.Mention}!", Color.Green.RawValue)
                        .Build());
                await db.SaveChangesAsync();
            }
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Enable/disable join/leaves logging, leave empty to disable")]
        public async Task LogJoinAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.LogJoin = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled logging of join/leave!", Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogJoin = channel.Id;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set join/leave logging channel to {channel.Mention}!",
                        Color.Green.RawValue).Build());
                await db.SaveChangesAsync();
            }
        }

        [Command("message", RunMode = RunMode.Async)]
        [Alias("msg")]
        [Summary("Enable/Disable message logging, leave empty to disable")]
        public async Task LogMessageAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.LogMsg = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled logging of messages!", Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogMsg = channel.Id;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set message logging channel to {channel.Mention}!",
                        Color.Green.RawValue).Build());
                await db.SaveChangesAsync();
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Alias("ban")]
        [Summary("Enable/Disable moderation logging, leave empty to disable")]
        public async Task LogBanAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.LogBan = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled logging of moderation actions!", Color.Green.RawValue)
                            .Build());
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogBan = channel.Id;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set mod log channel to {channel.Mention}!", Color.Green.RawValue)
                        .Build());
                await db.SaveChangesAsync();
            }
        }

        [Command("automod", RunMode = RunMode.Async)]
        [Summary("Enable/Disable separate logging of auto-moderator")]
        public async Task LogAutoModAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAutoMod = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled separate logging of auto-moderator actions!", Color.Green.RawValue)
                            .Build());
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAutoMod = channel.Id;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set auto mod log channel to {channel.Mention}!", Color.Green.RawValue)
                        .Build());
                await db.SaveChangesAsync();
            }
        }
    }
}