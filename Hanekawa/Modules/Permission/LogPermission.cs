﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Permission
{
    [Group("log")]
    [Summary("Manage logging settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class LogPermission : InteractiveBase
    {
        private readonly DbService _db;

        public LogPermission(DbService db)
        {
            _db = db;
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Summary("Enable/disable warn logging, leave empty to disable")]
        public async Task LogWarnAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogWarn = null;
                await Context.ReplyAsync("Disabled logging of warnings!", Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogWarn = channel.Id;
            await Context.ReplyAsync($"Set warn logging channel to {channel.Mention}!", Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Enable/disable join/leaves logging, leave empty to disable")]
        public async Task LogJoinAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogJoin = null;
                await Context.ReplyAsync("Disabled logging of join/leave!", Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogJoin = channel.Id;
            await Context.ReplyAsync($"Set join/leave logging channel to {channel.Mention}!",
                Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }

        [Command("message", RunMode = RunMode.Async)]
        [Alias("msg")]
        [Summary("Enable/Disable message logging, leave empty to disable")]
        public async Task LogMessageAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogMsg = null;
                await Context.ReplyAsync("Disabled logging of messages!", Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogMsg = channel.Id;
            await Context.ReplyAsync($"Set message logging channel to {channel.Mention}!",
                Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Alias("ban")]
        [Summary("Enable/Disable moderation logging, leave empty to disable")]
        public async Task LogBanAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogBan = null;
                await Context.ReplyAsync("Disabled logging of moderation actions!", Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogBan = channel.Id;
            await Context.ReplyAsync($"Set mod log channel to {channel.Mention}!", Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }

        [Command("automod", RunMode = RunMode.Async)]
        [Summary("Enable/Disable separate logging of auto-moderator")]
        public async Task LogAutoModAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogAutoMod = null;
                await Context.ReplyAsync("Disabled separate logging of auto-moderator actions!",
                    Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogAutoMod = channel.Id;
            await Context.ReplyAsync($"Set auto mod log channel to {channel.Mention}!", Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }

        [Command("user")]
        [Summary("Enable/Disable logging of user changes (avatar and name/nickname)")]
        public async Task LogUserUpdates(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null)
            {
                cfg.LogAvi = null;
                await Context.ReplyAsync("Disabled logging of user updates!",
                    Color.Green.RawValue);
                await _db.SaveChangesAsync();
                return;
            }

            cfg.LogAvi = channel.Id;
            await Context.ReplyAsync($"Set user update log channel to {channel.Mention}!", Color.Green.RawValue);
            await _db.SaveChangesAsync();
        }
    }
}