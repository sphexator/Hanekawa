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

        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [Summary("Permission overview")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ViewPermissionsAsync()
        {
            await Context.ReplyAsync("Currently disabled");
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [Alias("set prefix")]
        [Summary("Sets custom prefix for this guild/server")]
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

        [Command("embed")]
        [Alias("set embed")]
        [Summary("Sets a custom colour for embeds")]
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

        [Command("embed")]
        [Alias("set embed")]
        [Summary("Sets a custom colour for embeds")]
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

        [Command("embed")]
        [Alias("set embed")]
        [Summary("Sets a custom colour for embeds")]
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

        [Command("embed")]
        [Alias("set embed")]
        [Summary("Sets a custom colour for embeds")]
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