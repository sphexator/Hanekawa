using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Parsers;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.Board
{
    public partial class BoardService
    {
        public void SetBoardEmote(CachedGuild guild, string emote) =>
            _reactionEmote.AddOrUpdate(guild.Id, emote, (key, value) => emote);

        public async Task<IEmoji> GetEmote(CachedGuild guild, DbService db)
        {
            var check = _reactionEmote.TryGetValue(guild.Id, out var emoteString);
            if (!check)
            {
                var cfg = await db.GetOrCreateBoardConfigAsync(guild);
                if (LocalCustomEmoji.TryParse(cfg.Emote, out var dbEmote))
                {
                    _reactionEmote.TryAdd(guild.Id, dbEmote.MessageFormat);
                    return dbEmote;
                }

                cfg.Emote = null;
                await db.SaveChangesAsync();
                return new LocalEmoji("U+2B50");
            }

            if (LocalCustomEmoji.TryParse(emoteString, out var emote)) return emote;
            return new LocalEmoji("U+2B50");
        }
    }
}