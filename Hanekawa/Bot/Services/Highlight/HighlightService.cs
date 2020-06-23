using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Highlight
{
    public class HighlightService
    {
        private readonly DiscordBot _client;
        private readonly IServiceProvider _provider;

        private readonly ConcurrentDictionary<(ulong, ulong), string[]> _highlights =
            new ConcurrentDictionary<(ulong, ulong), string[]>();

        public HighlightService(DiscordBot client, IServiceProvider provider)
        {
            _client = client;
            _provider = provider;

            using var scope = _provider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            {
                foreach (var x in db.Highlights)
                    _highlights.TryAdd((x.GuildId, x.UserId), x.Highlights);
            }
        }

        public async Task<string[]> Add(CachedMember user, string[] text)
        {
            var highlights = _highlights.GetOrAdd((user.Guild.Id.RawValue, user.Id.RawValue), text);
            var newList = text.Where(x => !highlights.Contains(x)).ToList();
            var returnList = text.Where(x => highlights.Contains(x)).ToArray();
            if (newList.Count == 0) return returnList;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var highlightDb = await db.Highlights.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue);
            if (highlightDb == null)
            {
                await db.Highlights.AddAsync(new Database.Tables.Account.Highlight
                {
                    GuildId = user.Guild.Id.RawValue,
                    UserId = user.Id.RawValue,
                    Highlights = newList.ToArray()
                });
            }
            else highlightDb.Highlights = newList.ToArray();

            await db.SaveChangesAsync();
            _highlights.AddOrUpdate((user.Guild.Id.RawValue, user.Id.RawValue), newList.ToArray(), (tuple, strings) => newList.ToArray());
            return returnList;
        }
        
        public async Task<string[]> Remove(CachedMember user, string[] text)
        {
            if(!_highlights.TryGetValue((user.Guild.Id.RawValue, user.Id.RawValue), out var highlights)) return null;
            var newList = text.Where(x => highlights.Contains(x)).ToList();
            var returnList = text.Where(x => !highlights.Contains(x)).ToArray();
            if (newList.Count == 0) return returnList;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var highlightDb = await db.Highlights.FindAsync(user.Guild.Id.RawValue, user.Id.RawValue);
            
            if (highlightDb == null && newList.Count > 0)
            {
                await db.Highlights.AddAsync(new Database.Tables.Account.Highlight
                {
                    GuildId = user.Guild.Id.RawValue,
                    UserId = user.Id.RawValue,
                    Highlights = newList.ToArray()
                });
                _highlights.AddOrUpdate((user.Guild.Id.RawValue, user.Id.RawValue), newList.ToArray(), (tuple, strings) => newList.ToArray());
            }
            else if (highlightDb != null && newList.Count > 0)
            {
                highlightDb.Highlights = newList.ToArray();
                _highlights.AddOrUpdate((user.Guild.Id.RawValue, user.Id.RawValue), newList.ToArray(), (tuple, strings) => newList.ToArray());
            }
            else if (highlightDb != null && newList.Count == 0)
            {
                db.Highlights.Remove(highlightDb);
            } 

            await db.SaveChangesAsync();
            return returnList;
        }
    }
}
