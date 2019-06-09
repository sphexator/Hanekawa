using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Account.Economy
{
    [Name("Economy")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public partial class Economy : InteractiveBase
    {
        [Name("Regular Currency Name")]
        [Command("rcn")]
        [Description("Change the name of regular currency (default: credit)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetRegularNameAsync([Remainder] string name = null)
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

        [Name("Special Currency Name")]
        [Command("scn")]
        [Description("Change the name of special currency (default: special credit)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSpecialNameAsync([Remainder] string name = null)
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

        [Name("Regular Currency Symbol")]
        [Command("rcs")]
        [Description("Change the symbol of regular currency (default: $)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetRegularSymbolAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.EmoteCurrency = true;
                cfg.CurrencySign = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Regular Currency Symbol")]
        [Command("rcs")]
        [Description("Change the symbol of regular currency (default: $)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetRegularSymbolAsync([Remainder]string symbol)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (symbol.IsNullOrWhiteSpace()) symbol = "$";

                cfg.EmoteCurrency = false;
                cfg.CurrencySign = symbol;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency sign to {symbol}", Color.Green.RawValue);
            }
        }

        [Name("Special Currency Symbol")]
        [Command("scs")]
        [Description("Change the symbol of special currency (default: $)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSpecialSymbolAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.SpecialEmoteCurrency = true;
                cfg.SpecialCurrencySign = emote.ParseEmoteString();
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set special currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Name("Special Currency Symbol")]
        [Command("scs")]
        [Description("Change the symbol of special currency (default: $)")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetSpecialSymbolAsync([Remainder] string symbol)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (symbol.IsNullOrWhiteSpace()) symbol = "$";

                cfg.SpecialEmoteCurrency = false;
                cfg.SpecialCurrencySign = symbol;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set special currency sign to {symbol}", Color.Green.RawValue);
            }
        }
    }
}
