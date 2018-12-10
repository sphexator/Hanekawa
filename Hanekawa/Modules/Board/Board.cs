using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
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
        //TODO this need looks at for errors
        [Command("stats", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows board stats for server")]
        [RequiredChannel]
        public async Task BoardStatsAsync()
        {
            using (var db = new DbService())
            {


                var emote = _boardService.GetGuildEmote(Context.Guild);
                var amount = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var topStars = await db.Boards.Where(x => x.GuildId == Context.Guild.Id)
                    .OrderByDescending(x => x.StarAmount)
                    .Take(3).ToListAsync();
                var topRecieve = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id && x.Active)
                    .OrderByDescending(x => x.StarReceived).Take(3).ToListAsync();

                string topR = null;
                string topM = null;
                var topRr = 1;
                var topMr = 1;
                uint stars = 0;

                foreach (var x in amount) stars = stars + x.StarAmount;

                foreach (var x in topRecieve)
                    try
                    {
                        topR +=
                            $"{topRr}: {Context.Guild.GetUser(x.UserId).Mention ?? "N/A"} ({x.StarReceived} {emote})\n";
                        topRr++;
                    }
                    catch
                    {
                        topR += $"{topRr}: User left the server. ({x.UserId}) ({x.StarReceived} {emote})\n";
                        topRr++;
                    }

                foreach (var x in topStars)
                {
                    topM += $"{topMr}: {x.MessageId} ({x.StarAmount} {emote})\n";
                    topMr++;
                }

                var embed = new EmbedBuilder()
                    .CreateDefault($"{amount.Count} messages boarded with a total of {stars} {emote} given").WithAuthor(
                        new EmbedAuthorBuilder
                            {IconUrl = Context.Guild.IconUrl, Name = $"{Context.Guild.Name} board stats"});
                embed.AddField($"Top {emote} Posts", $"{topM ?? "N/A"}");
                embed.AddField($"Top {emote} Receivers", $"{topR ?? "N/A"}");
                await Context.ReplyAsync(embed);
            }
        }

        [Command("stats", RunMode = RunMode.Async)]
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
                    foreach (var x in boardData.OrderByDescending(x => x.StarAmount).Take(3))
                    {
                        topStar += $"{id} > {x.MessageId} ({emote} received {x.StarAmount}";
                        id++;
                    }
                else topStar += $"No {emote} messages";

                var embed = new EmbedBuilder().CreateDefault(null).WithAuthor(new EmbedAuthorBuilder
                    { IconUrl = user.GetAvatar(), Name = user.GetName() });
                embed.AddField("Boarded Messages", $"{boardData.Count}", true);
                embed.AddField($"{emote} Received", $"{userData.StarReceived}", true);
                embed.AddField($"{emote} Given", $"{userData.StarGiven}", true);
                embed.AddField($"Top {emote} Posts", $"{topStar ?? "N/A"}");
                await Context.ReplyAsync(embed);
            }
        }

        [Command("emote", RunMode = RunMode.Async)]
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
                await Context.ReplyAsync($"Changed board emote to {emote}", Color.Green.RawValue);
            }
        }

        [Command("channel", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Sets channel for board to be used in")]
        public async Task BoardChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.BoardChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled the board", Color.Green.RawValue);
                    return;
                }

                cfg.BoardChannel = channel.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set board channel to {channel.Mention}", Color.Green.RawValue);
            }
        }

        private static string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";
        }
    }
}