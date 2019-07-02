using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Board;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interactive;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Cooldown = Hanekawa.Shared.Cooldown;

namespace Hanekawa.Bot.Modules.Board
{
    [Name("Board")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Board : InteractiveBase
    {
        private readonly BoardService _service;
        private readonly InternalLogService _log;
        public Board(BoardService service, InternalLogService log)
        {
            _service = service;
            _log = log;
        }

        [Name("Board Stats")]
        [Command("boardstats")]
        [Description("Overview of board stats of this server")]
        [RequiredChannel]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.WhateverWithMoreSalt)]
        public async Task BoardStatsAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                var emote = cfg.Emote.ParseStringEmote();
                var boards = await db.Boards.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var topStars = boards.OrderByDescending(x => x.StarAmount).Take(3).ToList();
                var topReceive = await db.Accounts.Where(x => x.GuildId == Context.Guild.Id)
                    .OrderByDescending(x => x.StarReceived).Take(3).ToListAsync();
                var totalStars = 0;
                for (var i = 0; i < boards.Count; i++) totalStars += boards[i].StarAmount;
                
                string topReceived = null;
                for (var i = 0; i < topReceive.Count; i++)
                {
                    var index = topReceive[i];
                    var user = Context.Guild.GetUser(index.UserId);
                    topReceived += $"{i}: {user.Mention ?? "User left the server"} ({index.StarReceived} {emote})\n";
                }

                string topStarMessages = null;
                for (var i = 0; i < topStars.Count; i++)
                {
                    var index = topStars[i];
                    topStarMessages += $"{i}: {index.MessageId} ({index.StarAmount} {emote})\n";
                }
                var embed = new EmbedBuilder().CreateDefault(null, Context.Guild.Id);
                embed.Description = $"{boards.Count} messages boarded with a total of {totalStars} {emote} given";
                embed.AddField($"Top {emote} Posts", $"{topStarMessages ?? "N/A"}");
                embed.AddField($"Top {emote} Receivers", $"{topReceived ?? "N/A"}");
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Board Stats")]
        [Command("boardstats")]
        [Description("Shows board stats for a user")]
        [RequiredChannel]
        public async Task BoardStatsAsync(SocketGuildUser user)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                var emote = cfg.Emote.ParseStringEmote();
                var userData = await db.GetOrCreateUserData(user);
                var boardData = await db.Boards.Where(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id)
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
                else topStar += $"No {emote} messages";

                var embed = new EmbedBuilder().CreateDefault(null, Context.Guild.Id);
                embed.Author = new EmbedAuthorBuilder { IconUrl = user.GetAvatar(), Name = user.GetName() };
                embed.AddField("Boarded Messages", $"{boardData.Count}", true);
                embed.AddField($"{emote} Received", $"{userData.StarReceived}", true);
                embed.AddField($"{emote} Given", $"{userData.StarGiven}", true);
                embed.AddField($"Top {emote} Posts", $"{topStar ?? "N/A"}");
                await Context.ReplyAsync(embed);
            }
        }
    }
}
