using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Settings")]
    [Description("Server settings")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class Settings : HanekawaCommandModule
    {
        private readonly ColourService _colourService;
        private readonly Services.Caching.CacheService _cacheService;

        public Settings(ColourService colourService, Services.Caching.CacheService cacheService)
        {
            _colourService = colourService;
            _cacheService = cacheService;
        }

        [Name("Add cacheService")]
        [Command("addprefix", "aprefix")]
        [Description("Adds a cacheService to the bot, if it doesn't already exist")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var config = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            if (config.Prefix != prefix)
            {
                _cacheService.AddorUpdatePrefix(Context.Guild, prefix);
                config.Prefix = prefix;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {prefix} as a cacheService.", Color.Green);
                return;
            }
            await Context.ReplyAsync($"{prefix} is already a cacheService on this server.", Color.Red);
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(Color color)
        {
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", new Color(color));
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                x.Message.Guild == Context.Guild && x.Message.Author == Context.User);
            if (response == null)
            {
                await Context.ReplyAsync("Timed out...", Color.Red);
                return;
            }
            if (response.Message.Content.ToLower() == "y" || response.Message.Content.ToLower() == "yes")
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                _colourService.AddOrUpdate(Context.Guild.Id.RawValue, new Color(color));
                cfg.EmbedColor = (uint)color.RawValue;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Changed default embed color");
            }
            else
                await Context.ReplyAsync("Cancelled");
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task SetEmbedColorAsync(int r, int g, int b)
        {
            var color = new Color(r, g, b);
            await Context.ReplyAsync("Would you like to change embed color to this ? (y/n)", color);
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                x.Message.Guild == Context.Guild && x.Message.Author == Context.User);
            if (response == null)
            {
                await Context.ReplyAsync("Timed out...", Color.Red);
                return;
            }
            if (response.Message.Content.ToLower() == "y" || response.Message.Content.ToLower() == "yes")
                await using (var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                    _colourService.AddOrUpdate(Context.Guild.Id.RawValue, color);
                    cfg.EmbedColor = (uint)color.RawValue;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Changed default embed color");
                }
            else
                await Context.ReplyAsync("Canceled");
        }
    }
}