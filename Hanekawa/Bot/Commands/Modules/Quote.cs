using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Quote")]
    [Description("Commands for quotes")]
    public class Quote : HanekawaCommandModule
    {
        [Name("Quote")]
        [Command("quote", "q")]
        [Description("Sends a pre-defined quote")]
        [RequiredChannel]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public async Task<DiscordCommandResult> QuoteAsync([Remainder] string key)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var quote = await db.Quotes.FindAsync(Context.GuildId, key);
            return quote == null 
                ? Reply("No quote could be found with that key!", HanaBaseColor.Bad()) 
                : Response(quote.Message);
        }

        [Name("Quote Add")]
        [Command("quoteadd", "qa")]
        [Description("Adds a quote with a identifier")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public async Task<DiscordCommandResult> QuoteAddAsync(string key, [Remainder] string message)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entry = await db.Quotes.FindAsync(Context.GuildId, key);
            if (entry != null)
                return Reply("There's already a quote with that key!\n Please use a different key",
                    HanaBaseColor.Bad());

            await db.Quotes.AddAsync(new Database.Tables.Quote.Quote
            {
                GuildId = Context.Guild.Id.RawValue,
                Key = key,
                Message = message,
                Added = DateTimeOffset.UtcNow,
                Creator = Context.Author.Id,
                LevelCap = 0,
                Triggers = new List<string>()
            });
            await db.SaveChangesAsync();
            return Reply($"Added quote with key {key} !", HanaBaseColor.Ok());
        }

        [Name("Quote Remove")]
        [Command("quoteremove", "qr")]
        [Description("Remove a quote with a identifier")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public async Task<DiscordCommandResult> QuoteRemoveAsync(string key)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entry = await db.Quotes.FindAsync(Context.GuildId, key);
            if (entry == null) return Reply("Couldn't find a quote with that name !", HanaBaseColor.Bad());
            db.Quotes.Remove(entry);
            await db.SaveChangesAsync();
            return Reply($"Successfully removed a quote with key {key} !", HanaBaseColor.Ok());
        }

        [Name("Quote List")]
        [Command("quotelist", "ql")]
        [Description("Lists all quotes")]
        [RequiredChannel]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public async Task<DiscordMenuCommandResult> QuoteListAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var entries = await db.Quotes.Where(x => x.GuildId == Context.GuildId).ToArrayAsync();
            var pages = new List<string>();
            foreach (var x in entries)
            {
                var sb = new StringBuilder();
                var creator = await Context.Guild.GetOrFetchMemberAsync(x.Creator);
                sb.AppendLine($"Key: {x.Key}");
                sb.AppendLine($"Msg: {x.Message.Truncate(100)}");
                sb.AppendLine($"Author: {creator}");
            }

            return Pages(pages.Pagination(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"{Context.Guild.Name} Quotes"));
        }
    }
}