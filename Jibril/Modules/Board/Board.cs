using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Reaction;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Board
{
    [Group("board")]
    public class Board : InteractiveBase
    {
        private readonly BoardService _boardService;
        public Board(BoardService boardService)
        {
            _boardService = boardService;
        }

        [Command("stats")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows board stats for specific user")]
        [RequiredChannel]
        public async Task BoardStatsAsync()
        {
            using (var db = new DbService())
            {
                var amount = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).CountAsync();
                var topStars = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarAmount)
                    .Take(3).ToListAsync();
                var topGive = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarGiven).Take(3).ToListAsync();
                var topRecieve = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarReceived).Take(3).ToListAsync();
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = $"{Context.Guild.Name} board stats"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.DarkPurple
                };
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("stats")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows board stats for specific user")]
        [RequiredChannel]
        public async Task BoardStatsAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(user as SocketGuildUser);
                var boardData = await db.Boards.Where(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id)
                    .ToListAsync();
                string topStar = null;
                var id = 1;
                foreach (var x in boardData.OrderBy(x => x.StarAmount).Take(3))
                {
                    topStar += $"{id} > {x.MessageId} (Star received {x.StarAmount}";
                    id++;
                }
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.DarkPurple
                };
                embed.AddField("Messages Starred", $"{boardData.Count}", true);
                embed.AddField("Stars Received", $"{userData.StarReceived}", true);
                embed.AddField("Star Given", $"{userData.StarGiven}", true);
                embed.AddField("Top Starred Posts", topStar);
                await ReplyAsync(null, false, embed.Build());
            }
        }
        
        [Command("emote")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets a custom emote to be used toward the board")]
        public async Task BoardEmoteAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var emoteString = ParseEmoteString(emote);
                _boardService.SetBoardEmote(Context.Guild, emoteString);
                cfg.BoardEmote = emoteString;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Changed board emote to {emote}", Color.Green.RawValue).Build());
            }
        }

        private static string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<{emote.Name}:{emote.Id}>";
        }
    }
}