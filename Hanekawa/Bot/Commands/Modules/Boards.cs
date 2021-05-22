using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using static Disqord.LocalCustomEmoji;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Board")]
    [Group("Board")]
    [Description("Commands for managing the board")]
    public class Boards : HanekawaCommandModule
    {
        [Name("Board Stats")]
        [Command("stats")]
        [Description("Shows board stats for a user, if empty it'll be your own")]
        [RequiredChannel]
        public async Task BoardStatsAsync(IMember user = null)
        {
            user ??= Context.Author;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
            TryParse(cfg.Emote, out var emote);
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

            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {IconUrl = user.GetAvatarUrl(), Name = user.Name},
                Fields =
                {
                    new LocalEmbedFieldBuilder
                        {Name = "Boarded Messages", Value = $"{boardData.Count}", IsInline = true},
                    new LocalEmbedFieldBuilder
                        {Name = $"{emote} Received", Value = $"{userData.StarReceived}", IsInline = true},
                    new LocalEmbedFieldBuilder
                        {Name = $"{emote} Given", Value = $"{userData.StarGiven}", IsInline = true},
                    new LocalEmbedFieldBuilder {Name = $"Top {emote} Posts", Value = $"{topStar ?? "N/A"}"}
                }
            };
            await Reply(embed);
        }

        public class BoardAdmin : Boards, IModuleSetting
        {
            [Name("Board Emote")]
            [Command("emote")]
            [Description("Sets a emote to be used for the board")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task BoardEmoteAsync(IGuildEmoji emote)
            {
                var cache = Context.Services.GetRequiredService<CacheService>();
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                cfg.Emote = emote.GetMessageFormat();
                await db.SaveChangesAsync();
                cache.AddOrUpdateEmote(EmoteType.Board, Context.GuildId, emote);
                await Reply($"Changed board emote to {emote}", HanaBaseColor.Ok());
            }

            [Name("Board Channel")]
            [Command("channel")]
            [Description("Sets which channel starred messages go")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task<DiscordCommandResult> BoardChannelAsync(ITextChannel channel = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateBoardConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    return Reply("Disabled starboard", HanaBaseColor.Ok());
                }

                cfg.Channel = channel.Id;
                await db.SaveChangesAsync();
                return Reply($"Set board channel to {channel.Mention}", HanaBaseColor.Ok());
            }
        }
    }
}