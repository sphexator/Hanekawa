﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Bot.Services.Economy
{
    public class CurrencyService : INService
    {
        private readonly string[] _currencySigns = { "$", "€", "£" };

        public async Task<string> ToCurrency(DbService db, ulong guildId, int amount, bool isSpecial = false) 
            => ToCurrency(await db.GetOrCreateCurrencyConfigAsync(guildId), amount, isSpecial);

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

        private IEmote CurrencySignEmote(string emoteString)
        {
            if (Emote.TryParse(emoteString, out var emote)) return emote;
            Emote.TryParse("<a:wawa:475462796214009856>", out var defaultEmote);
            return defaultEmote;
        }
    }
}
