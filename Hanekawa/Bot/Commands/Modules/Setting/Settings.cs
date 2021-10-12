using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Name("Settings")]
    [Description("Bot settings for all services and commands")]
    [RequireBotGuildPermissions(Permission.SendMessages | Permission.SendEmbeds)]
    [RequireAuthorGuildPermissions(Permission.ManageGuild)]
    public class Settings : HanekawaCommandModule
    {
        private readonly CacheService _cache;
        public Settings(CacheService cache) => _cache = cache;

        [Name("Add Prefix")]
        [Command("prefix", "addprefix")]
        [Description("Adds a prefix to the bot, if it doesn't already exist")]
        public async Task<DiscordCommandResult> AddPrefixAsync([Remainder] string prefix)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var config = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            if (config.Prefix == prefix)
                return Reply($"{prefix} is already a prefix on this server.", HanaBaseColor.Bad());
            
            _cache.AddOrUpdatePrefix(Context.GuildId, new StringPrefix(prefix));
            config.Prefix = prefix;
            await db.SaveChangesAsync();
            return Reply($"Added {prefix} as a prefix.", HanaBaseColor.Ok());
        }

        [Name("Set embed color")]
        [Command("embed")]
        [Description("Changes the embed colour of the bot")]
        public async Task<DiscordCommandResult> SetEmbedColorAsync([Remainder] Color color)
        {
            await Reply("Would you like to change embed color to this ? (y/n)", new Color(color));
            var response = await Context.Bot.GetInteractivity()
                .WaitForMessageAsync(Context.ChannelId, e => e.Member.Id == Context.Author.Id);
            if (response == null) return Reply("Timed out...", HanaBaseColor.Bad());
            if (response.Message.Content.ToLower() != "y" && response.Message.Content.ToLower() != "yes")
                return Reply("Cancelled", HanaBaseColor.Bad());
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            _cache.AddOrUpdateColor(Context.Guild.Id, new Color(color));
            cfg.EmbedColor = color.RawValue;
            await db.SaveChangesAsync();
            return Reply("Changed default embed color", HanaBaseColor.Ok());
        }
    }
}