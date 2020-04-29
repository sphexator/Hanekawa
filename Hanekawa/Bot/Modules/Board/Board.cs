using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Board;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Board
{
    [Name("Board")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public partial class Board : HanekawaModule
    {
        private readonly InternalLogService _log;
        private readonly BoardService _service;

        public Board(BoardService service, InternalLogService log)
        {
            _service = service;
            _log = log;
        }

        [Name("Board Stats")]
        [Command("boardstats")]
        [Description("Overview of board stats of this server")]
        [RequiredChannel]
        public async Task BoardStatsAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                LocalCustomEmoji.TryParse(cfg.Emote, out var emote);
                var boards = await db.Boards.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
                var topStars = boards.OrderByDescending(x => x.StarAmount).Take(3).ToList();
                var topReceive = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id.RawValue)
                    .OrderByDescending(x => x.StarReceived).Take(3).ToListAsync();
                var totalStars = 0;
                for (var i = 0; i < boards.Count; i++) totalStars += boards[i].StarAmount;

                string topReceived = null;
                for (var i = 0; i < topReceive.Count; i++)
                {
                    var index = topReceive[i];
                    var user = Context.Guild.GetMember(index.UserId);
                    topReceived += $"{i}: {user.Mention ?? "User left the server"} ({index.StarReceived} {emote})\n";
                }

                string topStarMessages = null;
                for (var i = 0; i < topStars.Count; i++)
                {
                    var index = topStars[i];
                    topStarMessages += $"{i}: {index.MessageId} ({index.StarAmount} {emote})\n";
                }

                var embed = new LocalEmbedBuilder()
                {
                    Description = $"{boards.Count} messages boarded with a total of {totalStars} {emote} given",
                    Fields =
                    {
                        new LocalEmbedFieldBuilder { Name = $"Top {emote} Posts", Value = $"{topStarMessages ?? "N/A"}" },
                        new LocalEmbedFieldBuilder { Name = $"Top {emote} Receivers", Value = $"{topReceived ?? "N/A"}" }
                    }
                };
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Board Stats")]
        [Command("boardstats")]
        [Description("Shows board stats for a user")]
        [RequiredChannel]
        public async Task BoardStatsAsync(CachedMember user)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                LocalCustomEmoji.TryParse(cfg.Emote, out var emote);
                var userData = await db.GetOrCreateUserData(user);
                var boardData = await db.Boards.Where(x => x.GuildId == Context.Guild.Id.RawValue && x.UserId == user.Id.RawValue)
                    .OrderByDescending(x => x.StarAmount).ToListAsync();
                string topStar = null;
                if (boardData.Count != 0)
                {
                    if (boardData.Count > 3) boardData = boardData.Take(3).ToList();
                    for (var i = 0; i < boardData.Count; i++)
                    {
                        var index = boardData[i];
                        topStar += $"{i} > {index.MessageId} ({emote} received {index.StarAmount})\n";
                    }
                }
                else
                {
                    topStar += $"No {emote} messages";
                }

                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder {IconUrl = user.GetAvatarUrl(), Name = user.DisplayName},
                    Fields = 
                    {
                        new LocalEmbedFieldBuilder{ Name = "Boarded Messages", Value = $"{boardData.Count}", IsInline = true },
                        new LocalEmbedFieldBuilder{ Name = $"{emote} Received", Value = $"{userData.StarReceived}", IsInline = true },
                        new LocalEmbedFieldBuilder{ Name = $"{emote} Given", Value = $"{userData.StarGiven}", IsInline = true },
                        new LocalEmbedFieldBuilder{ Name = $"Top {emote} Posts", Value = $"{topStar ?? "N/A"}" }
                    }
                };
                await Context.ReplyAsync(embed);
            }
        }
    }
}