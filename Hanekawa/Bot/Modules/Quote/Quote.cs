using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Caching;
using Hanekawa.Database;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Quote
{
    public class Quote : HanekawaCommandModule
    {
        private readonly CacheService _cache;
        public Quote(CacheService cache) => _cache = cache;

        [Name("Quote")]
        [Command("quote", "q")]
        [Description("Sends a pre-defined quote")]
        [RequiredChannel]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.User)]
        public async Task QuoteAsync([Remainder] string key)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var quote = await db.Quotes.FindAsync(Context.Guild.Id.RawValue, key);
            if (quote == null) return;
            
            var cache = _cache.QuoteCache.GetOrAdd(Context.Guild.Id, new MemoryCache(new MemoryCacheOptions()));
            if (cache.TryGetValue(quote.Key, out var value) && (int)value == 0)
            {
                cache.Set(quote.Key, 1);
                _cache.QuoteCache.AddOrUpdate(Context.Guild.Id, cache, (_, _) => cache);
                await ReplyAsync("This quote is still on cooldown...");
                return;
            }

            cache.Set(quote.Key, 0, TimeSpan.FromMinutes(1));
            _cache.QuoteCache.AddOrUpdate(Context.Guild.Id, cache, (_, _) => cache);
            await ReplyAsync(quote.Message);
        }

        [Name("Quote Add")]
        [Command("quoteadd", "qa")]
        [Description("Adds a quote with a identifier")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task QuoteAddAsync(string key, [Remainder] string message)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entry = await db.Quotes.FindAsync(Context.Guild.Id.RawValue, key);
            if (entry != null)
            {
                await ReplyAsync("There's already a quote with that key!\n" +
                                 "Please use a different key", Color.Red);
                return;
            }

            await db.Quotes.AddAsync(new Database.Tables.Quote.Quote
            {
                GuildId = Context.Guild.Id.RawValue,
                Key = key,
                Message = message,
                Added = DateTimeOffset.UtcNow,
                Creator = Context.User.Id.RawValue,
                LevelCap = 0,
                Triggers = new List<string>()
            });
            await db.SaveChangesAsync();
            await ReplyAsync($"Added quote with key {key} !", HanaBaseColor.Lime());
        }

        [Name("Quote Remove")]
        [Command("quoteremove", "qr")]
        [Description("Remove a quote with a identifier")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task QuoteRemoveAsync(string key)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entry = await db.Quotes.FindAsync(Context.Guild.Id.RawValue, key);
            if (entry == null)
            {
                await ReplyAsync("Couldn't find a quote with that name !", Color.Red);
                return;
            }

            db.Quotes.Remove(entry);
            await db.SaveChangesAsync();
            await ReplyAsync($"Successfully removed a quote with key {key} !", HanaBaseColor.Lime());
        }

        [Name("Quote List")]
        [Command("quotelist", "ql")]
        [Description("Lists all quotes")]
        [RequiredChannel]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.User)]
        public async Task QuoteListAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entries = await db.Quotes.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            var pages = new List<string>();
            foreach (var x in entries)
            {
                var sb = new StringBuilder();
                var creator = Context.Guild.GetMember(x.Creator);
                sb.AppendLine($"Key: {x.Key}");
                sb.AppendLine($"Msg: {x.Message.Truncate(100)}");
                sb.AppendLine(creator != null ? $"Author: {creator}" : $"Author: {x.Creator}");
            }
            await Context.PaginatedReply(pages, Context.Guild, $"Quotes for {Context.Guild.Name}");
        }
    }
}