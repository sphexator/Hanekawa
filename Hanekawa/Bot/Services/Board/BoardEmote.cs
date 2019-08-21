using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService
    {
        public void SetBoardEmote(SocketGuild guild, string emote) =>
            _reactionEmote.AddOrUpdate(guild.Id, emote, (key, value) => emote);

        public async Task<IEmote> GetEmote(SocketGuild guild, DbService db)
        {
            var check = _reactionEmote.TryGetValue(guild.Id, out var emoteString);
            if (!check)
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(guild);
                if (Emote.TryParse(cfg.Emote, out var dbEmote))
                {
                    _reactionEmote.TryAdd(guild.Id, dbEmote.ParseEmoteString());
                    return dbEmote;
                }

                cfg.Emote = null;
                await db.SaveChangesAsync();
                return new Emoji("⭐");
            }

            if (Emote.TryParse(emoteString, out var emote)) return emote;
            return new Emoji("⭐");
        }
    }
}