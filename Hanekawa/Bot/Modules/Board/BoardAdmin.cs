using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Board
{
    public partial class Board
    {
        [Name("Board Emote")]
        [Command("boardemote")]
        [Description("Sets a emote to be used for the board")]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task BoardEmoteAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild); 
                cfg.Emote = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Changed board emote to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Board Channel")]
        [Command("boardchannel")]
        [Description("Sets which channel starred messages go")]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task BoardChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled starboard", Color.Green.RawValue);
                }
                else
                {
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set board channel to {channel.Mention}", Color.Green.RawValue);
                }
            }
        }
    }
}
