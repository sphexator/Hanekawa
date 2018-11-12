﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Database.Tables.Stores;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Account.Storage
{
    public class ShopManager
    {
        public async Task<IEnumerable<string>> GetServerStoreAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var result = new List<string>();
                var store = await db.Shops.Where(x => x.GuildId == user.GuildId).ToListAsync();
                var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                for (var i = 0; i < store.Count;)
                {
                    string page = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == store.Count) continue;

                        var storeContent = store[i];
                        page +=
                            $"Name: {(await db.Items.FindAsync(storeContent.ItemId)).Name} (id:{storeContent.ItemId}) - {GetPrice(cfg, storeContent, storeContent.Price)}\n";
                        i++;
                    }

                    result.Add(page);
                }

                if (result.Count == 0) result.Add("Store is empty");
                return result;
            }
        }

        public async Task<IEnumerable<string>> GetGlobalStoreAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var result = new List<string>();
                var store = await db.StoreGlobals.ToListAsync();
                for (var i = 0; i < store.Count;)
                {
                    string page = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == store.Count) continue;

                        var storeContent = store[i];
                        page +=
                            $"Item: {(await db.Items.FindAsync(storeContent.ItemId)).Name} (id:{storeContent.ItemId}) - Price: ${storeContent.Price}\n";
                        i++;
                    }

                    result.Add(page);
                }

                return result;
            }
        }

        public async Task<EmbedBuilder> PurchaseItem(DbService db, Shop shop, IGuildUser user)
        {
            var price = shop.Price;
            var item = await db.Items.FindAsync(shop.ItemId);
            return await PurchaseServer(db, item, price, shop.SpecialCredit, user);
        }

        public async Task<EmbedBuilder> PurchaseItem(DbService db, StoreGlobal shop, IGuildUser user)
        {
            var price = shop.Price;
            var item = await db.Items.FindAsync(shop.ItemId);
            return await PurchaseGlobal(db, item, price, user);
        }

        private static async Task<EmbedBuilder> PurchaseGlobal(DbService db, Item item, int price, IGuildUser user)
        {
            var userdata = await db.GetOrCreateGlobalUserData(user.Id);

            if (userdata.Credit < price)
                return new EmbedBuilder().Reply($"{user.Mention}, you don't have enough credit to buy {item.Name}",
                    Color.Red.RawValue);

            return await BuyNormalCredit(db, item, price, user, null, userdata);
        }

        private static async Task<EmbedBuilder> PurchaseServer(DbService db, Item item, int price, bool specialCredit,
            IGuildUser user)
        {
            var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
            if (specialCredit && userdata.CreditSpecial < price)
                return new EmbedBuilder().Reply($"{user.Mention}, you don't have enough credit to buy {item.Name}",
                    Color.Red.RawValue);

            if (!specialCredit && userdata.Credit < price)
                return new EmbedBuilder().Reply($"{user.Mention}, you don't have enough credit to buy {item.Name}",
                    Color.Red.RawValue);

            if (specialCredit) return await BuySpecialCredit(db, item, price, user, userdata);
            return await BuyNormalCredit(db, item, price, user, userdata, null);
        }

        private static async Task<EmbedBuilder> BuySpecialCredit(DbService db, Item item, int price, IGuildUser user,
            Addons.Database.Tables.Account.Account account)
        {
            account.Credit = account.Credit - (uint) price;
            await db.PurchaseServerItem(user, item);
            await db.SaveChangesAsync();
            return new EmbedBuilder().Reply($"{user.Mention} purchased {item.Name} for {price}");
        }

        private static async Task<EmbedBuilder> BuyNormalCredit(DbService db, Item item, int price, IGuildUser user,
            Addons.Database.Tables.Account.Account account = null, AccountGlobal accountGlobal = null)
        {
            if (accountGlobal == null)
            {
                account.Credit = account.Credit - (uint) price;
                await db.PurchaseServerItem(user, item);
                await db.SaveChangesAsync();
                return new EmbedBuilder().Reply($"{user.Mention} purchased {item.Name} for {price}");
            }

            accountGlobal.Credit = accountGlobal.Credit - (uint) price;
            await db.PurchaseGlobalItem(user, item);
            await db.SaveChangesAsync();
            return new EmbedBuilder().Reply($"{user.Mention} purchased {item.Name} for {price}");
        }

        private static string GetPrice(GuildConfig cfg, Shop shop, int price)
        {
            return shop.SpecialCredit ? GetSpecialCurrency(price, cfg) : GetRegularCurrency(price, cfg);
        }

        private static string SpecialCurrencyResponse(GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency ? $"{CurrencySignEmote(cfg.SpecialCurrencySign)}" : cfg.SpecialCurrencySign;
        }

        private static string GetRegularCurrency(int price, GuildConfig cfg)
        {
            return cfg.EmoteCurrency
                ? EmoteCurrencyResponse(price, cfg.CurrencyName, cfg.CurrencySign)
                : RegularCurrencyResponse(price, cfg.CurrencyName, cfg.CurrencySign);
        }

        private static string GetSpecialCurrency(int price, GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency
                ? EmoteCurrencyResponse(price, cfg.SpecialCurrencyName, cfg.SpecialCurrencySign)
                : RegularCurrencyResponse(price, cfg.SpecialCurrencyName, cfg.SpecialCurrencySign);
        }

        private static string RegularCurrencyResponse(int credit,
            string name, string sign)
        {
            return $"{name}: {sign}{credit}";
        }

        private static string EmoteCurrencyResponse(int credit, string name, string sign)
        {
            return $"{name}: {credit} {sign}";
        }

        private static IEmote CurrencySignEmote(string emoteString)
        {
            if (Emote.TryParse(emoteString, out var emote)) return emote;

            Emote.TryParse("<a:wawa:475462796214009856>", out var defaultEmote);
            return defaultEmote;
        }
    }
}