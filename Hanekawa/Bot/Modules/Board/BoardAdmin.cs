using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Qmmands;

namespace Hanekawa.Bot.Modules.Board
{
    public partial class Board
    {
        [Name("Board Emote")]
        [Command("boardemote")]
        [Description("Sets a emote to be used for the board")]
        [RequireBotGuildPermissions(Permission.ManageGuild)]
        public async Task BoardEmoteAsync(Emoji emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                cfg.Emote = emote.MessageFormat;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Changed board emote to {emote}", Color.Green);
            }
        }

        [Name("Board Channel")]
        [Command("boardchannel")]
        [Description("Sets which channel starred messages go")]
        [RequireBotGuildPermissions(Permission.ManageGuild)]
        public async Task BoardChannelAsync(CachedTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled starboard", Color.Green);
                }
                else
                {
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set board channel to {channel.Mention}", Color.Green);
                }
            }
        }
    }
}