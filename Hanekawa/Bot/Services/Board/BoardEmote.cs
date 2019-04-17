using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Hanekawa.Database.Extensions;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService
    {
        public void SetBoardEmote(SocketGuild guild, string emote) =>
            _reactionEmote.AddOrUpdate(guild.Id, emote, (key, value) => emote);

        public async Task<IEmote> GetEmote(SocketGuild guild)
        {
            var check = _reactionEmote.TryGetValue(guild.Id, out var emoteString);
            if (!check)
            {
                var cfg = await _db.GetOrCreateBoardConfigAsync(guild);
                if (Emote.TryParse(cfg.Emote, out var dbEmote))
                {
                    _reactionEmote.TryAdd(guild.Id, cfg.Emote);
                    return dbEmote;
                }

                cfg.Emote = null;
                await _db.SaveChangesAsync();
                return new Emoji("⭐");
            }

            if (Emote.TryParse(emoteString, out var emote)) return emote;
            return new Emoji("⭐");
        }
    }
}
