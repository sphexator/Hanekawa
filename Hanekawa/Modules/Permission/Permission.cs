using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.CommandHandler;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    public class Permission : InteractiveBase
    {
        private readonly DefaultColors _colors;
        private readonly CommandHandlingService _command;

        public Permission(CommandHandlingService command, DefaultColors colors)
        {
            _command = command;
            _colors = colors;
        }

        [Name("Set prefix")]
        [Command("prefix", RunMode = RunMode.Async)]
        [Alias("set prefix")]
        [Summary("**Require Administrator**\nSets custom prefix for this guild/server")]
        [Remarks("h.prefix !")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix)
        {
            try
            {
                await _command.UpdatePrefixAsync(Context.Guild, prefix);
                await Context.ReplyAsync($"Successfully changed prefix to {prefix}", Color.Green.RawValue);
            }
            catch
            {
                await Context.ReplyAsync($"Something went wrong changing prefix to {prefix}",
                    Color.Red.RawValue);
            }
        }

        [Name("Embed color")]
        [Command("embed")]
        [Alias("set embed")]
        [Summary("**Require Manage Server**\nSets a custom colour for embeds")]
        [Remarks("h.embed 16669612")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEmbed(uint color)
        {
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    cfg.EmbedColor = color;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                    cfg.UpdateConfig(Context.Guild.Id);
                }
            else
                await Context.ReplyAsync("Canceled");
        }

        [Name("Embed color")]
        [Command("embed")]
        [Alias("set embed")]
        [Summary("**Require Manage Server**\nSets a custom colour for embeds")]
        [Remarks("h.embed 99 00 00")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEmbed(int r, int g, int b)
        {
            var color = new Color(r, g, b).RawValue;
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    cfg.EmbedColor = color;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                    cfg.UpdateConfig(Context.Guild.Id);
                }
            else
                await Context.ReplyAsync("Canceled");
        }

        [Name("Embed color")]
        [Command("embed")]
        [Alias("set embed")]
        [Summary("**Require Manage Server**\nSets a custom colour for embeds")]
        [Remarks("h.embed pink")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEmbed(Colors type)
        {
            var color = _colors.GetColor(type).RawValue;
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    cfg.EmbedColor = color;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                    cfg.UpdateConfig(Context.Guild.Id);
                }
            else
                await Context.ReplyAsync("Canceled");
        }

        [Name("Embed")]
        [Command("embed")]
        [Alias("set embed")]
        [Summary("**Require Manage Server**\nSets a custom colour for embeds")]
        [Remarks("h.embed #ff0022")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEmbed(string hex)
        {
            var color = _colors.GetColor(hex).RawValue;
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    cfg.EmbedColor = color;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                    cfg.UpdateConfig(Context.Guild.Id);
                }
            else
                await Context.ReplyAsync("Canceled");
        }
    }
}