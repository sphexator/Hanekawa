using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Quartz.Util;

namespace Hanekawa.Modules.Account
{
    [Name("Currency")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class AdminEconomy : InteractiveBase
    {
        [Name("Regular currency name")]
        [Command("currency regular name", RunMode = RunMode.Async)]
        [Alias("crn")]
        [Summary("Change the name of regular currency (default: credit)")]
        [Remarks("h.crn credit")]
        public async Task SetCurrencyNameAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.CurrencyName = "Credit";
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set regular back to default `Credit`", Color.Green.RawValue);
                    return;
                }

                cfg.CurrencyName = name;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency name to {name}", Color.Green.RawValue);
            }
        }

        [Name("Regular currency symbol")]
        [Command("currency regular symbol", RunMode = RunMode.Async)]
        [Alias("crs")]
        [Summary("Change the symbol of regular currency to an emote (default: $")]
        [Remarks("h.crs <emote>")]
        [Priority(1)]
        public async Task SetCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.EmoteCurrency = true;
                cfg.CurrencySign = emote.ParseToString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Regular currency symbol")]
        [Command("currency regular symbol", RunMode = RunMode.Async)]
        [Alias("crs")]
        [Summary("Changes the symbol of a regular currency (default: $)")]
        [Remarks("h.crs $")]
        public async Task SetCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.EmoteCurrency = false;
                    cfg.CurrencyName = "$";
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set currency sign back to default `$`", Color.Green.RawValue);
                    return;
                }

                cfg.EmoteCurrency = false;
                cfg.CurrencySign = name;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency sign to {name}", Color.Green.RawValue);
            }
        }

        [Name("Currency special name")]
        [Command("currency special name", RunMode = RunMode.Async)]
        [Alias("csn")]
        [Summary("Changes the name of special currency (default: Special credit)")]
        [Remarks("h.csn special credit")]
        public async Task SetSpecialCurrencyNameAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.SpecialCurrencyName = "Special Credit";
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set regular back to default `Special Credit`",
                        Color.Green.RawValue);
                    return;
                }

                cfg.SpecialCurrencyName = name;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency name to {name}", Color.Green.RawValue);
            }
        }

        [Name("Currency special symbol")]
        [Command("currency special symbol", RunMode = RunMode.Async)]
        [Alias("css")]
        [Summary("Changes the symbol of special currency to an emote (default: $)")]
        [Remarks("h.css <emote>")]
        [Priority(1)]
        public async Task SetSpecialCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.SpecialEmoteCurrency = true;
                cfg.SpecialCurrencySign = emote.ParseToString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Currency special symbol")]
        [Command("currency special symbol", RunMode = RunMode.Async)]
        [Alias("css")]
        [Summary("Changes the symbol of special currency (default: $)")]
        [Remarks("h.css $")]
        public async Task SetSpecialCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace()) name = "$";

                cfg.SpecialEmoteCurrency = false;
                cfg.SpecialCurrencySign = name;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency sign to {name}", Color.Green.RawValue);
            }
        }
    }
}