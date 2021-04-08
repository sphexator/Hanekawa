using System.Linq;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Extensions
{
    public static class CurrencyExtensions
    {
        private static readonly string[] _currencySigns = {"$", "€", "£"};

        public static string ToCurrencyFormat(this CurrencyConfig cfg, int amount, bool isSpecial = false)
        {
            if (isSpecial)
                return cfg.SpecialEmoteCurrency
                    ? ParseEmote(cfg.SpecialCurrencySign, amount)
                    : ParseString(cfg.SpecialCurrencySign, amount);
            return cfg.EmoteCurrency ? ParseEmote(cfg.CurrencySign, amount) : ParseString(cfg.CurrencySign, amount);
        }

        private static string ParseEmote(string sign, int amount) => $"{amount} {CurrencySignEmote(sign)}";

        private static string ParseString(string sign, int amount) =>
            _currencySigns.Contains(sign)
                ? $"{sign}{amount}"
                : $"{amount} {sign}";

        private static IEmoji CurrencySignEmote(string emoteString)
        {
            if (LocalCustomEmoji.TryParse(emoteString, out var emote)) return emote;
            LocalCustomEmoji.TryParse("<a:wawa:475462796214009856>", out var defaultEmote);
            return defaultEmote;
        }
    }
}