using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Highlight
{
    public class HighlightService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colour;
        private readonly ConcurrentDictionary<(ulong, ulong), string[]> _highlights =
            new ConcurrentDictionary<(ulong, ulong), string[]>();

        public HighlightService(DiscordSocketClient client, IServiceProvider provider, ColourService colour)
        {
            _client = client;
            _provider = provider;
            _colour = colour;

            using var db = new DbService();
            {
                foreach (var x in db.Highlights)
                    _highlights.TryAdd((x.GuildId, x.UserId), x.Highlights);
            }
            
            _client.MessageReceived += message =>
            {
                _ = OnMessageReceived(message);
                return Task.CompletedTask;
            };
        }

        public async Task<string[]> Add(SocketGuildUser user, string[] text)
        {
            var highlights = _highlights.GetOrAdd((user.Guild.Id, user.Id), text);
            var newList = text.Where(x => !highlights.Contains(x)).ToList();
            var returnList = text.Where(x => highlights.Contains(x)).ToArray();
            if (newList.Count == 0) return returnList;
            using var db = new DbService();
            var highlightDb = await db.Highlights.FindAsync(user.Guild.Id, user.Id);
            if (highlightDb == null)
            {
                await db.Highlights.AddAsync(new Database.Tables.Account.Highlight
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id,
                    Highlights = newList.ToArray()
                });
            }
            else highlightDb.Highlights = newList.ToArray();

            await db.SaveChangesAsync();
            _highlights.AddOrUpdate((user.Guild.Id, user.Id), newList.ToArray(), (tuple, strings) => newList.ToArray());
            return returnList;
        }
        
        public async Task<string[]> Remove(SocketGuildUser user, string[] text)
        {
            if(!_highlights.TryGetValue((user.Guild.Id, user.Id), out var highlights)) return null;
            var newList = text.Where(x => highlights.Contains(x)).ToList();
            var returnList = text.Where(x => !highlights.Contains(x)).ToArray();
            if (newList.Count == 0) return returnList;
            using var db = new DbService();
            var highlightDb = await db.Highlights.FindAsync(user.Guild.Id, user.Id);
            
            if (highlightDb == null && newList.Count > 0)
            {
                await db.Highlights.AddAsync(new Database.Tables.Account.Highlight
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id,
                    Highlights = newList.ToArray()
                });
                _highlights.AddOrUpdate((user.Guild.Id, user.Id), newList.ToArray(), (tuple, strings) => newList.ToArray());
            }
            else if (highlightDb != null && newList.Count > 0)
            {
                highlightDb.Highlights = newList.ToArray();
                _highlights.AddOrUpdate((user.Guild.Id, user.Id), newList.ToArray(), (tuple, strings) => newList.ToArray());
            }
            else if (highlightDb != null && newList.Count == 0)
            {
                db.Highlights.Remove(highlightDb);
            } 

            await db.SaveChangesAsync();
            return returnList;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            return; // TODO: Complete highlight
            if (!(message is SocketUserMessage msg)) return;
            if (!(msg.Author is SocketGuildUser guildUser)) return;
            if (guildUser.IsBot) return;
            var words = message.Content.Split(" ");
            var guildKeywords = _highlights.Where(x => x.Key.Item1 == guildUser.Guild.Id);
            var pairs = guildKeywords as KeyValuePair<(ulong, ulong), string[]>[] ?? guildKeywords.ToArray();
            var keywords = pairs.SelectMany(e => e.Value).ToArray();
            var highlightList = new Dictionary<ulong, string[]>();
            foreach (var x in pairs) highlightList.TryAdd(x.Key.Item2, x.Value);
            
            var highlighted = new List<string>();
            for (var i = 0; i < words.Length; i++)
            {
                var x = words[i];
                if(keywords.Contains(x)) highlighted.Add(x);
            }

            if (highlighted.Count == 0) return;
            var messages = message.Channel.GetCachedMessages(message.Id, Direction.Before, 5);
            var valuePairs = highlightList.Where(x => x.Value.Intersect(highlighted).Any());

            var str = new StringBuilder();
            for (var i = 0; i < messages.Count; i++)
            {
                var e = messages.ElementAt(i);
                str.AppendLine($"{e.Author.Mention}: {e.Content}");
            }
            var embed = new EmbedBuilder().Create(str.ToString(), _colour.Get(guildUser.Guild.Id));
            foreach (var x in valuePairs)
            {
                var user = guildUser.Guild.GetUser(x.Key);
                if(user == null) continue;
                // https://www.discordapp.com/channels/438952486418645004/454003994398949416
            }
        }
    }
}
