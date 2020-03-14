using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services;
using Hanekawa.Bot.Services.Board;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Cooldown = Hanekawa.Shared.Command.Cooldown;

namespace Hanekawa.Bot.Modules.Board
{
    [Name("Board")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Board : DiscordModuleBase<HanekawaContext>
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

                var embed = new EmbedBuilder
                {
                    Description = $"{boards.Count} messages boarded with a total of {totalStars} {emote} given",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder { Name = $"Top {emote} Posts", Value = $"{topStarMessages ?? "N/A"}" },
                        new EmbedFieldBuilder { Name = $"Top {emote} Receivers", Value = $"{topReceived ?? "N/A"}" }
                    }
                };
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
                else
                {
                    topStar += $"No {emote} messages";
                }

                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder {IconUrl = user.GetAvatar(), Name = user.GetName()},
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder{ Name = "Boarded Messages", Value = $"{boardData.Count}", IsInline = true },
                        new EmbedFieldBuilder{ Name = $"{emote} Received", Value = $"{userData.StarReceived}", IsInline = true },
                        new EmbedFieldBuilder{ Name = $"{emote} Given", Value = $"{userData.StarGiven}", IsInline = true },
                        new EmbedFieldBuilder{ Name = $"Top {emote} Posts", Value = $"{topStar ?? "N/A"}" }
                    }
                };
                await Context.ReplyAsync(embed);
            }
        }
    }
}