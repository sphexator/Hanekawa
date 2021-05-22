using System.Linq;
using Disqord;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Exceptions;
using static Disqord.LocalCustomEmoji;

namespace Hanekawa.Extensions
{
    public static class CurrencyExtensions
    {
        private static readonly string[] CurrencySigns = {"$", "€", "£"};

        public static string ToCurrencyFormat(this CurrencyConfig cfg, int amount, bool isSpecial = false)
        {
            if (isSpecial)
                return cfg.SpecialEmoteCurrency
                    ? ParseEmote(cfg.SpecialCurrencySign, amount)
                    : ParseString(cfg.SpecialCurrencySign, amount);
            return cfg.EmoteCurrency ? ParseEmote(cfg.CurrencySign, amount) : ParseString(cfg.CurrencySign, amount);
        }

        private static string ParseEmote(string sign, int amount) => $"{amount.FormatCurrency()} {CurrencySignEmote(sign)}";

        private static string ParseString(string sign, int amount) =>
            CurrencySigns.Contains(sign)
                ? $"{sign}{amount.FormatCurrency()}"
                : $"{amount.FormatCurrency()} {sign}";

        private static IEmoji CurrencySignEmote(string emoteString)
        {
            if (TryParse(emoteString, out var emote)) return emote;
            return TryParse("<a:wawa:475462796214009856>", out var defaultEmote)
                ? defaultEmote
                : throw new HanaCommandException("Default currency emote couldn't be found");
        }
    }
}