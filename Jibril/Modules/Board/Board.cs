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
                var emote = _boardService.GetGuildEmote(Context.Guild);
                var amount = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var topStars = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarAmount)
                    .Take(3).ToListAsync();
                var topGive = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarGiven).Take(3).ToListAsync();
                var topRecieve = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.StarReceived).Take(3).ToListAsync();

                string topG = null;
                string topR = null;
                string topM = null;
                var topGr = 1;
                var topRr = 1;
                var topMr = 1;
                uint stars = 0;

                foreach (var x in amount)
                {
                    stars = stars + x.StarAmount;
                }

                foreach (var x in topRecieve)
                {
                    topR += $"{topRr} > {Context.Guild.GetUser(x.UserId).Mention ?? "N/A"} ({x.StarReceived} {emote})";
                    topRr++;
                }

                foreach (var x in topGive)
                {
                    topG += $"{topGr} > {Context.Guild.GetUser(x.UserId).Mention ?? "N/A"} ({x.StarReceived} {emote})";
                    topGr++;
                }

                foreach (var x in topStars)
                {
                    topM += $"{topMr} > {x.MessageId} ({x.StarAmount} {emote})";
                    topMr++;
                }

                var author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = $"{Context.Guild.Name} board stats"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.Purple,
                    Description = $"{amount.Count} messages boarded with a total of {stars} {emote} given"
                };
                embed.AddField($"Top {emote} Posts", topM);
                embed.AddField($"Top {emote} Receivers", topR);
                embed.AddField($"Top {emote} Givers", topG);
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
                var emote = _boardService.GetGuildEmote(Context.Guild);
                var userData = await db.GetOrCreateUserData(user as SocketGuildUser);
                var boardData = await db.Boards.Where(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id)
                    .ToListAsync();
                string topStar = null;
                var id = 1;
                if (boardData.Count != 0)
                {
                    foreach (var x in boardData.OrderBy(x => x.StarAmount).Take(3))
                    {
                        topStar += $"{id} > {x.MessageId} ({emote} received {x.StarAmount}";
                        id++;
                    }
                }
                else topStar += $"No {emote} messages";

                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.Purple
                };
                embed.AddField("Boarded Messages", $"{boardData.Count}", true);
                embed.AddField($"{emote} Received", $"{userData.StarReceived}", true);
                embed.AddField($"{emote} Given", $"{userData.StarGiven}", true);
                embed.AddField($"Top {emote} Posts", topStar);
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