using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Currency
{
    public class CurrencyService : IHanaService
    {
        private readonly string[] _currencySigns = {"$", "€", "£"};

        public async Task<string> ToCurrency(ulong guildId, int amount, bool isSpecial = false)
        {
            using (var db = new DbService())
            {
                return ToCurrency(await db.GetOrCreateCurrencyConfigAsync(guildId), amount, isSpecial);
            }
        }

        public string ToCurrency(CurrencyConfig cfg, int amount, bool isSpecial = false)
        {
            if (isSpecial)
                return cfg.SpecialEmoteCurrency
                    ? ParseEmote(cfg.SpecialCurrencySign, amount)
                    : ParseString(cfg.SpecialCurrencySign, amount);
            return cfg.EmoteCurrency ? ParseEmote(cfg.CurrencySign, amount) : ParseString(cfg.CurrencySign, amount);
        }

        private string ParseEmote(string sign, int amount) => $"{amount} {CurrencySignEmote(sign)}";

        private string ParseString(string sign, int amount) =>
            _currencySigns.Contains(sign)
                ? $"{sign}{amount}"
                : $"{amount} {sign}";

        private static IEmote CurrencySignEmote(string emoteString)
        {
            if (Emote.TryParse(emoteString, out var emote)) return emote;
            Emote.TryParse("<a:wawa:475462796214009856>", out var defaultEmote);
            return defaultEmote;
        }
    }
}