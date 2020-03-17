using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Settings")]
    [Description("Server settings")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class Settings : DiscordModuleBase<HanekawaContext>
    {
        private readonly ColourService _colourService;

        public Settings(ColourService colourService)
        {
            _colourService = colourService;
        }

        [Name("Add prefix")]
        [Command("addprefix", "aprefix")]
        [Description("Adds a prefix to the bot, if it doesn't already exist")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            using var db = new DbService();
            var prefixes = (Context.Bot.PrefixProvider as DefaultPrefixProvider).Prefixes.Contains(new StringPrefix());
            if (.AddPrefix(Context.Guild.Id, prefix, db))
                await Context.ReplyAsync($"Added {prefix} as a prefix.", Color.Green);
            else await Context.ReplyAsync($"{prefix} is already a prefix on this server.", Color.Red);
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(uint color)
        {
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", new Color(color));
            var response = await NextMessageAsync();
            if (response == null)
            {
                await Context.ReplyAsync("Timed out...", Color.Red);
                return;
            }
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    _colourService.AddOrUpdate(Context.Guild.Id, new Color(color));
                    cfg.EmbedColor = color;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                }
            else
                await Context.ReplyAsync("Canceled");
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(int r, int g, int b)
        {
            var color = new Color(r, g, b);
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response == null)
            {
                await Context.ReplyAsync("Timed out...", Color.Red);
                return;
            }
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    _colourService.AddOrUpdate(Context.Guild.Id, color);
                    cfg.EmbedColor = color.RawValue;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                }
            else
                await Context.ReplyAsync("Canceled");
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(string colorHex)
        {
            if (colorHex.Contains("#")) colorHex = colorHex.Replace("#", "");
            colorHex = colorHex.Insert(0, "0x");
            var color = new Color(Convert.ToInt32(colorHex, 16)); // _colors.GetColor(colorHex).RawValue;
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await NextMessageAsync();
            if (response == null)
            {
                await Context.ReplyAsync("Timed out...", Color.Red);
                return;
            }
            if (response.Content.ToLower() == "y" || response.Content.ToLower() == "yes")
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    _colourService.AddOrUpdate(Context.Guild.Id, color);
                    cfg.EmbedColor = color.RawValue;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                }
            else
                await Context.ReplyAsync("Canceled");
        }
    }
}