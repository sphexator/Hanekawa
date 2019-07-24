using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private readonly ConcurrentDictionary<ulong, Emote> _emotes = new ConcurrentDictionary<ulong, Emote>();

        public async Task ChangeEmote(SocketGuild guild, Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateDropConfig(guild);
                cfg.Emote = emote.ParseEmoteString();
                _emotes.AddOrUpdate(guild.Id, GetDefaultEmote(), (key, value) => emote);
                await db.SaveChangesAsync();
            }
        }

        private async Task<List<Emote>> ReturnEmotes(SocketGuild guild, DbService db)
        {
            var emotes = guild.Roles.Count >= 4
                ? guild.Emotes.ToList()
                : _client.GetGuild(431617676859932704).Emotes.ToList();

            var result = new List<Emote>();
            for (var x = 0; x < 4; x++)
            {
                var emote = emotes[_random.Next(emotes.Count)];
                if (result.Contains(emote))
                {
                    x--;
                    continue;
                }

                result.Add(emote);
            }

            result.Add(_emotes.GetOrAdd(guild.Id, await GetClaimEmote(guild, db)));
            return result;
        }

        private async Task<Emote> GetClaimEmote(SocketGuild guild, DbService db)
        {
            var cfg = await db.GetOrCreateDropConfig(guild);
            var isEmote = Emote.TryParse(cfg.Emote, out var emote);
            return isEmote ? emote : GetDefaultEmote();
        }

        private Emote GetDefaultEmote()
        {
            Emote.TryParse("<:realsip:429809346222882836>", out var real);
            return real;
        }
    }
}