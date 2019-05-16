using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Suggestion
{
    public partial class Suggestion
    {
        [Name("Suggestion Channel")]
        [Command("suggest channel")]
        [Description("Sets a channel as channel to receive suggestions. don't mention a channel to disable suggestions.")]
        [Remarks("suggest channel #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestionChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (cfg.Channel.HasValue && channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled suggestion channel", Color.Green.RawValue);
                    return;
                }
                if (channel == null) channel = Context.Channel;
                cfg.Channel = channel.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"All suggestions will now be sent to {channel.Mention} !",
                    Color.Green.RawValue);
            }
        }

        [Name("Suggest Yes Emote")]
        [Command("suggest set yes")]
        [Description("Set custom yes emote for suggestions")]
        [Remarks("suggestion set yes :yes:")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestEmoteYesAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteYes = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green.RawValue);
                    return;
                }

                cfg.EmoteYes = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Suggest No Emote")]
        [Command("suggest set no")]
        [Description("Set custom no emote for suggestions")]
        [Remarks("suggestion set yes :no:")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSuggestEmoteNoAsync(Emote emote = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateSuggestionConfigAsync(Context.Guild);
                if (emote == null)
                {
                    cfg.EmoteNo = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set `no` reaction to default emote", Color.Green.RawValue);
                    return;
                }

                cfg.EmoteNo = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set `no` reaction to {emote}", Color.Green.RawValue);
            }
        }
    }
}
