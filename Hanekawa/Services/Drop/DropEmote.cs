using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Drop
{
    public class DropEmote : IHanaService
    {
        private readonly DiscordSocketClient _client;
        private readonly Random _random;

        public DropEmote(DiscordSocketClient client, Random random)
        {
            _client = client;
            _random = random;
        }

        public List<Emote> ReturnEmotes()
        {
            var emotes = new List<Emote>();
            Emote.TryParse("<:realsip:429809346222882836>", out var real);
            Emote.TryParse("<:sip:430207651998334977>", out var sip1);
            Emote.TryParse("<:roowut:430207652061118465>", out var sip2);
            Emote.TryParse("<:rooWhine:430207965153460254>", out var sip3);

            emotes.Add(real);
            emotes.Add(sip1);
            emotes.Add(sip2);
            emotes.Add(sip3);

            return emotes;
        }

        private async Task<List<Emote>> GetEmotes(ulong guildId)
        {
            var result = new List<Emote>();
            foreach (var x in _client.GetGuild(123123).Emotes)
            {
                if (result.Count >= 3) continue;
                if (result.Contains(x)) continue;
                result.Add(x);
            }

            result.Add(await ClaimEmote(guildId));
            return result;
        }

        private async Task<Emote> ClaimEmote(ulong guildId)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(guildId);
                // Change board emote to new drop emote
                var isEmote = Emote.TryParse(cfg.BoardEmote, out var emote);
                return isEmote ? emote : GetDefaultEmote();
            }
        }

        private Emote GetDefaultEmote()
        {
            Emote.TryParse("<:realsip:429809346222882836>", out var real);
            return real;
        }
    }
}