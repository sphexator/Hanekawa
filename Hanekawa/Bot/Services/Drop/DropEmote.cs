using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private readonly ConcurrentDictionary<ulong, IEmoji> _emotes = new ConcurrentDictionary<ulong, IEmoji>();

        public async Task ChangeEmote(CachedGuild guild, LocalCustomEmoji emote)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateDropConfig(guild);
            cfg.Emote = emote.MessageFormat;
            _emotes.AddOrUpdate(guild.Id.RawValue, emote, (key, value) => emote);
            await db.SaveChangesAsync();
        }

        private async Task<List<IEmoji>> ReturnEmotes(CachedGuild guild, DbService db)
        {
            var emotes = guild.Emojis.Count >= 4
                ? guild.Emojis.ToList()
                : _client.GetGuild(431617676859932704).Emojis.ToList();

            var result = new List<IEmoji>();
            for (var x = 0; x < 4; x++)
            {
                var (_, emote) = emotes[_random.Next(emotes.Count)];
                if (result.Contains(emote) || !emote.IsAvailable)
                {
                    x--;
                    continue;
                }
                result.Add(emote);
            }

            result.Add(_emotes.TryGetValue(guild.Id.RawValue, out var claimEmote)
                ? claimEmote
                : _emotes.GetOrAdd(guild.Id.RawValue, await GetClaimEmote(guild, db)));
            return result;
        }

        private async Task<IEmoji> GetClaimEmote(CachedGuild guild, DbService db)
        {
            var cfg = await db.GetOrCreateDropConfig(guild);
            var isEmote = LocalCustomEmoji.TryParse(cfg.Emote, out var emote);
            return isEmote ? emote : GetDefaultEmote();
        }

        private IEmoji GetDefaultEmote()
        {
            LocalCustomEmoji.TryParse("<:realsip:429809346222882836>", out var real);
            return real;
        }
    }
}