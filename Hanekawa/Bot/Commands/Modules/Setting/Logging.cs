using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Group("Log", "Logging")]
    public class Logging : HanekawaCommandModule
    { 
        [Name("Join/Leave")]
        [Command("join")]
        [Description("Logs users joining and leaving server, leave empty to disable")]
        public async Task<DiscordCommandResult> JoinLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogJoin = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of join/leave!", HanaBaseColor.Ok());
            }

            cfg.LogJoin = channel.Id;
            return Reply($"Set join/leave logging channel to {channel.Mention}!", HanaBaseColor.Ok());
            await db.SaveChangesAsync();
        }

        [Name("Warnings")]
        [Command("warn")]
        [Description("Logs warnings given out from the bot, leave empty to disable")]
        public async Task<DiscordCommandResult> WarnLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogWarn = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of warnings!", HanaBaseColor.Ok());
            }

            cfg.LogWarn = channel.Id;
            return Reply($"Set warn logging channel to {channel.Mention}!", HanaBaseColor.Ok());
            await db.SaveChangesAsync();
        }

        [Name("Messages")]
        [Command("msgs")]
        [Description("Logs deleted and updated messages, leave empty to disable")]
        public async Task<DiscordCommandResult> MessageLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogMsg = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of messages!", HanaBaseColor.Ok());
            }

            cfg.LogMsg = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set message logging channel to {channel.Mention}!", HanaBaseColor.Ok());
        }

        [Name("Ban")]
        [Command("ban")]
        [Description("Logs users getting banned and muted, leave empty to disable")]
        public async Task<DiscordCommandResult> BanLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogBan = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of bans!", HanaBaseColor.Ok());
            }

            cfg.LogBan = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set ban logging channel to {channel.Mention}!", HanaBaseColor.Ok());
        }

        [Name("User")]
        [Command("user")]
        [Description("Logs user updates, roles/username/nickname, leave empty to disable")]
        public async Task<DiscordCommandResult> UserLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogAvi = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of users!", HanaBaseColor.Ok());
            }

            cfg.LogAvi = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set user logging channel to {channel.Mention}!", HanaBaseColor.Ok());
        }

        [Name("Auto-Moderator")]
        [Command("automod")]
        [Description(
            "Logs activities auto moderator does. Defaults to ban log if this is disabled. Meant if automod entries should be in a different channel.\nLeave empty to disable")]
        public async Task<DiscordCommandResult> AutoModeratorLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogAutoMod = null;
                await db.SaveChangesAsync();
                return Reply("Disabled separate logging of auto-moderator actions!",
                    HanaBaseColor.Ok());
            }

            cfg.LogAutoMod = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set auto mod log channel to {channel.Mention}!", HanaBaseColor.Ok());
        }

        [Name("Voice")]
        [Command("voice")]
        [Description("Logs voice activities, join/leave/mute/deafen, leave empty to disable")]
        public async Task<DiscordCommandResult> VoiceLogAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.LogVoice = null;
                await db.SaveChangesAsync();
                return Reply("Disabled logging of voice activity!", HanaBaseColor.Ok());
            }

            cfg.LogVoice = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set voice activity logging channel to {channel.Mention}!",
                HanaBaseColor.Ok());
        }
    }
}