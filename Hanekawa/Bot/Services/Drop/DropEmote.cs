using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Extensions;

namespace Hanekawa.Bot.Services.Drop
{
    public partial class DropService
    {
        private readonly ConcurrentDictionary<ulong, Emote> _emotes = new ConcurrentDictionary<ulong, Emote>();

        public void ChangeEmote(SocketGuild guild, Emote emote) 
            => _emotes.AddOrUpdate(guild.Id, GetDefaultEmote(), (key, value) => emote);

        private async Task<List<Emote>> ReturnEmotes(SocketGuild guild)
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

            result.Add(_emotes.GetOrAdd(guild.Id, await GetClaimEmote(guild)));
            return result;
        }

        private async Task<Emote> GetClaimEmote(SocketGuild guild)
        {
            var cfg = await _db.GetOrCreateDropConfig(guild);
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
