using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Log")]
    [Description("Log various actions users do to a channel of your choice so you have a better overview of the server.")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class Log : HanekawaCommandModule
    {
        [Name("Join/Leave")]
        [Command("logjoin")]
        [Description("Logs users joining and leaving server, leave empty to disable")]
        public async Task JoinLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogJoin = null;
                await Context.ReplyAsync("Disabled logging of join/leave!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogJoin = channel.Id.RawValue;
            await Context.ReplyAsync($"Set join/leave logging channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Warnings")]
        [Command("logwarn")]
        [Description("Logs warnings given out from the bot, leave empty to disable")]
        public async Task WarnLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogWarn = null;
                await Context.ReplyAsync("Disabled logging of warnings!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogWarn = channel.Id.RawValue;
            await Context.ReplyAsync($"Set warn logging channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Messages")]
        [Command("logmsgs")]
        [Description("Logs deleted and updated messages, leave empty to disable")]
        public async Task MessageLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogMsg = null;
                await Context.ReplyAsync("Disabled logging of messages!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogMsg = channel.Id.RawValue;
            await Context.ReplyAsync($"Set message logging channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Ban")]
        [Command("logban")]
        [Description("Logs users getting banned and muted, leave empty to disable")]
        public async Task BanLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogBan = null;
                await Context.ReplyAsync("Disabled logging of bans!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogBan = channel.Id.RawValue;
            await Context.ReplyAsync($"Set ban logging channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("User")]
        [Command("loguser")]
        [Description("Logs user updates, roles/username/nickname, leave empty to disable")]
        public async Task UserLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogAvi = null;
                await Context.ReplyAsync("Disabled logging of users!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogAvi = channel.Id.RawValue;
            await Context.ReplyAsync($"Set user logging channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Auto-Moderator")]
        [Command("logautomod")]
        [Description(
            "Logs activities auto moderator does. Defaults to ban log if this is disabled. Meant if automod entries should be in a different channel.\nLeave empty to disable")]
        public async Task AutoModeratorLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogAutoMod = null;
                await Context.ReplyAsync("Disabled separate logging of auto-moderator actions!",
                    Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogAutoMod = channel.Id.RawValue;
            await Context.ReplyAsync($"Set auto mod log channel to {channel.Mention}!", Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Voice")]
        [Command("logvoice")]
        [Description("Logs voice activities, join/leave/mute/deafen, leave empty to disable")]
        public async Task VoiceLogAsync(CachedTextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogVoice = null;
                await Context.ReplyAsync("Disabled logging of voice activity!", Color.Green);
                await db.SaveChangesAsync();
                return;
            }

            cfg.LogVoice = channel.Id.RawValue;
            await Context.ReplyAsync($"Set voice activity logging channel to {channel.Mention}!",
                Color.Green);
            await db.SaveChangesAsync();
        }

        [Name("Reaction")]
        [Command("logreaction", "logreac")]
        [Description(
            "Logs reaction activity, when people add or remove a message it'll be logged to specified channel. Provide no channel to disable.")]
        public async Task LogReactionAsync(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (!cfg.LogReaction.HasValue && channel != null)
            {
                var webhook = await channel.GetWebhooksAsync();
                var check = webhook.FirstOrDefault(x => x.Owner.Id != Context.Guild.CurrentMember.Id);
                RestWebhook client;
                if (check == null) client = await channel.CreateWebhookAsync("Hanekawa Log");
                else client = check;
                // CFG Null yada yada
                return;
            }
            if (channel == null)
            {

                return;
            }

            if (channel.Id.RawValue != cfg.LogReaction.Value)
            {

                return;
            }
        }
    }
}