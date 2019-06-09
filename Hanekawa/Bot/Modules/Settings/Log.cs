﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Log")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Log : InteractiveBase
    {
        [Name("Join/Leave")]
        [Command("log join")]
        [Description("Logs users joining and leaving server, leave empty to disable")]
        [Remarks("log join #log")]
        public async Task JoinLogAsync(SocketTextChannel channel = null)
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
                await Context.ReplyAsync($"Set join/leave logging channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Warnings")]
        [Command("log warn")]
        [Description("Logs warnings given out from the bot, leave empty to disable")]
        [Remarks("log warn #log")]
        public async Task WarnLogAsync(SocketTextChannel channel = null)
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

        [Name("Messages")]
        [Command("log messages", "log msgs")]
        [Description("Logs deleted and updated messages, leave empty to disable")]
        [Remarks("log msgs #log")]
        public async Task MessageLogAsync(SocketTextChannel channel = null)
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
                await Context.ReplyAsync($"Set message logging channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Ban")]
        [Command("log ban")]
        [Description("Logs users getting banned and muted, leave empty to disable")]
        [Remarks("log ban #log")]
        public async Task BanLogAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogBan = null;
                    await Context.ReplyAsync("Disabled logging of bans!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogBan = channel.Id;
                await Context.ReplyAsync($"Set ban logging channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("User")]
        [Command("log user")]
        [Description("Logs user updates, roles/username/nickname, leave empty to disable")]
        [Remarks("log user #log")]
        public async Task UserLogAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogAvi = null;
                    await Context.ReplyAsync("Disabled logging of users!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogAvi = channel.Id;
                await Context.ReplyAsync($"Set user logging channel to {channel.Mention}!", Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }

        [Name("Auto-Moderator")]
        [Command("log autmod")]
        [Description(
            "Logs activities auto moderator does. Defaults to ban log if this is disabled. Meant if automod entries should be in a different channel.\n Leave empty to disable")]
        [Remarks("log automod #log")]
        public async Task AutoModeratorLogAsync(SocketTextChannel channel = null)
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

        [Name("Voice")]
        [Command("log voice")]
        [Description("Logs voice activities, join/leave/mute/deafen, leave empty to disable")]
        [Remarks("log voice #log")]
        public async Task VoiceLogAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateLoggingConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.LogVoice = null;
                    await Context.ReplyAsync("Disabled logging of voice activity!", Color.Green.RawValue);
                    await db.SaveChangesAsync();
                    return;
                }

                cfg.LogVoice = channel.Id;
                await Context.ReplyAsync($"Set voice activity logging channel to {channel.Mention}!",
                    Color.Green.RawValue);
                await db.SaveChangesAsync();
            }
        }
    }
}