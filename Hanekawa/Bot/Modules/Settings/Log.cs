using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Log")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Log : InteractiveBase
    {
        [Name("Join/Leave")]
        [Command("logjoin")]
        [Description("Logs users joining and leaving server, leave empty to disable")]
        public async Task JoinLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogJoin = null;
                    await Context.ReplyAsync("Disabled logging of join/leave!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogJoin = channel.Id;
                await Context.ReplyAsync($"Set join/leave logging channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("Warnings")]
        [Command("logwarn")]
        [Description("Logs warnings given out from the bot, leave empty to disable")]
        public async Task WarnLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogWarn = null;
                    await Context.ReplyAsync("Disabled logging of warnings!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogWarn = channel.Id;
                await Context.ReplyAsync($"Set warn logging channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("Messages")]
        [Command("logmsgs")]
        [Description("Logs deleted and updated messages, leave empty to disable")]
        public async Task MessageLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogMsg = null;
                    await Context.ReplyAsync("Disabled logging of messages!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogMsg = channel.Id;
                await Context.ReplyAsync($"Set message logging channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("Ban")]
        [Command("logban")]
        [Description("Logs users getting banned and muted, leave empty to disable")]
        public async Task BanLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogBan = null;
                    await Context.ReplyAsync("Disabled logging of bans!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogBan = channel.Id;
                await Context.ReplyAsync($"Set ban logging channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("User")]
        [Command("loguser")]
        [Description("Logs user updates, roles/username/nickname, leave empty to disable")]
        public async Task UserLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAvi = null;
                    await Context.ReplyAsync("Disabled logging of users!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAvi = channel.Id;
                await Context.ReplyAsync($"Set user logging channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("Auto-Moderator")]
        [Command("logautomod")]
        [Description(
            "Logs activities auto moderator does. Defaults to ban log if this is disabled. Meant if automod entries should be in a different channel.\n Leave empty to disable")]
        public async Task AutoModeratorLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAutoMod = null;
                    await Context.ReplyAsync("Disabled separate logging of auto-moderator actions!",
                        Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAutoMod = channel.Id;
                await Context.ReplyAsync($"Set auto mod log channel to {channel.Mention}!", Color.Green);
                await db.SaveChangesAsync();
            }
        }

        [Name("Voice")]
        [Command("logvoice")]
        [Description("Logs voice activities, join/leave/mute/deafen, leave empty to disable")]
        public async Task VoiceLogAsync(SocketTextChannel channel = null)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogVoice = null;
                    await Context.ReplyAsync("Disabled logging of voice activity!", Color.Green);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogVoice = channel.Id;
                await Context.ReplyAsync($"Set voice activity logging channel to {channel.Mention}!",
                    Color.Green);
                await db.SaveChangesAsync();
            }
        }
    }
}