using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Account.Economy
{
    [Name("Economy")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public partial class Economy : HanekawaCommandModule
    {
        [Name("Regular Currency Name")]
        [Command("rcn")]
        [Description("Change the name of regular currency (default: credit)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetRegularNameAsync([Remainder] string name = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (name.IsNullOrWhiteSpace())
            {
                cfg.CurrencyName = "Credit";
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Set regular back to default `Credit`", Color.Green);
                return;
            }

            cfg.CurrencyName = name;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set regular currency name to {name}", Color.Green);
        }

        [Name("Special Currency Name")]
        [Command("scn")]
        [Description("Change the name of special currency (default: special credit)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSpecialNameAsync([Remainder] string name = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (name.IsNullOrWhiteSpace())
            {
                cfg.SpecialCurrencyName = "Special Credit";
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Set regular back to default `Special Credit`",
                    Color.Green);
                return;
            }

            cfg.SpecialCurrencyName = name;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set regular currency name to {name}", Color.Green);
        }

        [Name("Regular Currency Symbol")]
        [Command("rcs")]
        [Description("Change the symbol of regular currency (default: $)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetRegularSymbolAsync(LocalCustomEmoji emote)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            cfg.EmoteCurrency = true;
            cfg.CurrencySign = emote.MessageFormat;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set regular currency sign to {emote}", Color.Green);
        }

        [Name("Regular Currency Symbol")]
        [Command("rcs")]
        [Description("Change the symbol of regular currency (default: $)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetRegularSymbolAsync([Remainder] string symbol)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (symbol.IsNullOrWhiteSpace()) symbol = "$";

            cfg.EmoteCurrency = false;
            cfg.CurrencySign = symbol;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set regular currency sign to {symbol}", Color.Green);
        }

        [Name("Special Currency Symbol")]
        [Command("scs")]
        [Description("Change the symbol of special currency (default: $)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSpecialSymbolAsync(LocalCustomEmoji emote)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            cfg.SpecialEmoteCurrency = true;
            cfg.SpecialCurrencySign = emote.MessageFormat;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set special currency sign to {emote}", Color.Green);
        }

        [Name("Special Currency Symbol")]
        [Command("scs")]
        [Description("Change the symbol of special currency (default: $)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetSpecialSymbolAsync([Remainder] string symbol)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (symbol.IsNullOrWhiteSpace()) symbol = "$";

            cfg.SpecialEmoteCurrency = false;
            cfg.SpecialCurrencySign = symbol;
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Set special currency sign to {symbol}", Color.Green);
        }
    }
}