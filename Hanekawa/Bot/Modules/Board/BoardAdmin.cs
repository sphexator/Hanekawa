using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Board
{
    public partial class Board
    {
        [Name("Board Emote")]
        [Command("boardemote")]
        [Description("Sets a emote to be used for the board")]
        [RequireBotGuildPermissions(Permission.ManageGuild)]
        public async Task BoardEmoteAsync(LocalCustomEmoji emote)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
            cfg.Emote = emote.MessageFormat;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Changed board emote to {emote}", Color.Green);
        }

        [Name("Board Channel")]
        [Command("boardchannel")]
        [Description("Sets which channel starred messages go")]
        [RequireBotGuildPermissions(Permission.ManageGuild)]
        public async Task BoardChannelAsync(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.Channel = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled starboard", Color.Green);
            }
            else
            {
                cfg.Channel = channel.Id.RawValue;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set board channel to {channel.Mention}", Color.Green);
            }
        }
    }
}