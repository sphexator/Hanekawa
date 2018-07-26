using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Board
{
    [Group("board")]
    public class Board : InteractiveBase
    {
        [Command("stats")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows board stats for specific user")]
        [RequiredChannel(339383206669320192)]
        public async Task BoardStats()
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
            }
        }

        [Command("stats")]
        [RequireContext(ContextType.Guild)]
        [Summary("Shows board stats for specific user")]
        [RequiredChannel(339383206669320192)]
        public async Task BoardStats(IGuildUser user)
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
        public async Task BoardEmote(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.BoardEmote = emote.Id;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Changed board emote to {emote}", Color.Green.RawValue).Build());
                //TODO: Make concurrentDictionary on boardService and update emote to this
            }
        }
    }
}
