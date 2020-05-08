using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Suggestion
{
    public partial class Suggestion
    {
        [Name("Suggestion Channel")]
        [Command("ssc", "sschannel")]
        [Description(
            "Sets a channel as channel to receive suggestions. don't mention a channel to disable suggestions.")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSuggestionChannelAsync(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (cfg.Channel.HasValue && channel == null)
            {
                cfg.Channel = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled suggestion channel", Color.Green);
                return;
            }

            if (channel == null) channel = Context.CachedChannel;
            cfg.Channel = channel.Id.RawValue;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"All suggestions will now be sent to {channel.Mention} !",
                Color.Green);
        }

        [Name("Suggest Yes Emote")]
        [Command("ssy", "ssyes")]
        [Description("Set custom yes emote for suggestions")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSuggestEmoteYesAsync(LocalCustomEmoji emote = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (emote == null)
            {
                cfg.EmoteYes = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green);
                return;
            }

            cfg.EmoteYes = emote.MessageFormat;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green);
        }

        [Name("Suggest No Emote")]
        [Command("ssn", "ssno")]
        [Description("Set custom no emote for suggestions")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSuggestEmoteNoAsync(LocalCustomEmoji emote = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
            if (emote == null)
            {
                cfg.EmoteNo = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green);
                return;
            }

            cfg.EmoteNo = emote.MessageFormat;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green);
        }
    }
}