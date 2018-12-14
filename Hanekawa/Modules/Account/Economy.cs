﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Addons.Database.Tables.Stores;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Modules.Account.Storage;
using Hanekawa.Preconditions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Account
{
    [RequireContext(ContextType.Guild)]
    public class Economy : InteractiveBase
    {
        private readonly InventoryManager _inventoryManager;
        private readonly ShopManager _shopManager;

        public Economy(ShopManager shopManager, InventoryManager inventoryManager)
        {
            _shopManager = shopManager;
            _inventoryManager = inventoryManager;
        }

        [Command("wallet")]
        [Alias("balance", "money")]
        [Summary("Display how much credit you got")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                if (user == null) user = Context.User as SocketGuildUser;
                var userdata = await db.GetOrCreateUserData(user);
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                var embed = new EmbedBuilder()
                    .CreateDefault($"{GetRegularCurrency(userdata, cfg)}\n" +
                                   $"{GetSpecialCurrency(userdata, cfg)}", Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {IconUrl = user.GetAvatar(), Name = user.GetName()});
                await Context.ReplyAsync(embed);
            }
        }

        [Command("give", RunMode = RunMode.Async)]
        [Alias("transfer")]
        [Summary("Transfer credit between users")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task GiveCreditAsync(uint amount, SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (userdata.Credit < amount)
                {
                    await Context.ReplyAsync($"{Context.User.Mention} doesn't have enough credit",
                        Color.Red.RawValue);
                    return;
                }
                var receiverData = await db.GetOrCreateUserData(user);

                userdata.Credit = userdata.Credit - amount;
                receiverData.Credit = receiverData.Credit + amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"{Context.User.Mention} transferred ${amount} to {user.Mention}",
                    Color.Green.RawValue);
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Summary("Daily credit command, usable once every 18hrs")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task DailyAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                    await Context.ReplyAsync($"{Context.User.Mention} daily credit refresh in {timer.Humanize()}",
                        Color.Red.RawValue);
                    return;
                }

                uint reward;
                if (user == null || user == Context.User)
                {
                    user = Context.User as SocketGuildUser;
                    reward = 200;
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.DailyCredit = DateTime.UtcNow;
                    userdata.Credit = userdata.Credit + reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"rewarded {user?.Mention} with ${reward} credit", Color.Green.RawValue);
                }
                else
                {
                    reward = Convert.ToUInt32(new Random().Next(200, 300));
                    var userData = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var recieverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    recieverData.Credit = recieverData.Credit + reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"{Context.User.Mention} rewarded {user.Mention} with ${reward} credit");
                }
            }
        }

        [Command("richest", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds)]
        [Summary("Displays top 10 users on the money leaderboard")]
        [RequiredChannel]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                        {Name = $"Money leaderboard for {Context.Guild.Name}", IconUrl = Context.Guild.IconUrl}
                };
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                var rank = 1;
                foreach (var x in await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                    .OrderByDescending(account => account.Credit).Take(10).ToListAsync())
                {
                    embed.AddField(new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value =
                            $"{Context.Client.GetUser(x.UserId).Mention ?? $"User left server (ID: {x.UserId})"} - {cfg.CurrencyName}:{x.Credit}"
                    });
                    rank++;
                }

                await Context.ReplyAsync(embed);
            }
        }

        [Command("reward", RunMode = RunMode.Async)]
        [Alias("award")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Rewards special credit to users (does not remove from yourself)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RewardCreditAsync(uint amount, IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                userdata.CreditSpecial = userdata.CreditSpecial + amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Rewarded {SpecialCurrencyResponse(cfg)}{amount} {cfg.SpecialCurrencyName} to {user.Mention}",
                    Color.Green.RawValue);
            }
        }

        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [Summary("Inventory of user")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task InventoryAsync() =>
            await Context.ReplyAsync(await _inventoryManager.GetInventory(Context.User as IGuildUser));

        [Command("store", RunMode = RunMode.Async)]
        [Alias("shop")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Displays the server store")]
        [RequiredChannel]
        public async Task ServerShopAsync()
        {
            await PagedReplyAsync((await _shopManager.GetServerStoreAsync(Context.User as IGuildUser)).ToList()
                .PaginateBuilder(Context.Guild.Id, Context.Guild, $"Store for {Context.Guild.Name}"));
        }

        [Command("global store", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Displays the global store")]
        [RequiredChannel]
        public async Task GlobalShopAsync()
        {
            await PagedReplyAsync((await _shopManager.GetGlobalStoreAsync(Context.User as IGuildUser)).PaginateBuilder(Context.Guild.Id, "Global Store"));
        }

        [Command("buy", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Purchase an item from the store")]
        [RequiredChannel]
        public async Task BuyAsync(int itemId)
        {
            using (var db = new DbService())
            {
                var item = await db.Shops.FindAsync(Context.Guild.Id, itemId);
                var globalItem = await db.StoreGlobals.FindAsync(Context.Guild.Id, itemId);
                if (item == null && globalItem == null)
                {
                    await Context.ReplyAsync("No item with said id");
                    return;
                }

                EmbedBuilder embed;
                if (item != null) embed = await _shopManager.PurchaseItem(db, item, Context.User as IGuildUser);
                else embed = await _shopManager.PurchaseItem(db, globalItem, Context.User as IGuildUser);
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("item description", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Sets a description of item")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetItemDescription(int id, [Remainder] string description)
        {
            using (var db = new DbService())
            {
                var item = await db.Items.FindAsync(id);
                if (item.GuildId.HasValue && item.GuildId != Context.Guild.Id) return;
                item.Description = description;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Updated description of {item.Name}");
            }
        }

        [Command("item inspect", RunMode = RunMode.Async)]
        [Alias("inspect item")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Sets a description of item")]
        [RequiredChannel]
        public async Task InspectItem(int id)
        {
            using (var db = new DbService())
            {
                var item = await db.Items.FindAsync(id);
                if (item.GuildId.HasValue && item.GuildId != Context.Guild.Id) return;
                var embed = new EmbedBuilder()
                    .CreateDefault(item.Description, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {Name = item.Name});

                embed.AddField("Role", item.Role.HasValue, true);
                embed.AddField("Unique", item.Unique, true);
                if (item.Role.HasValue)
                {
                    var role = Context.Guild.GetRole(item.Role.Value);
                    embed.AddField("Color", $"{role.Color}");
                    embed.AddField("Hoisted", $"{role.IsHoisted}", true);
                }
            }
        }

        [Command("store add", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Priority(1)]
        [Summary("Purchase an item from the store")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddStoreItemAsync(IRole role, int price, bool special = false)
        {
            using (var db = new DbService())
            {
                var date = DateTime.UtcNow;
                var item = new Item
                {
                    Global = false,
                    GuildId = Context.Guild.Id,
                    Name = role.Name,
                    Role = role.Id,
                    Secret = false,
                    SecretValue = null,
                    Unique = true,
                    Value = null,
                    ConsumeOnUse = false,
                    DateAdded = date
                };
                await db.Items.AddAsync(item);
                await db.SaveChangesAsync();

                var getItem = await db.Items.FirstOrDefaultAsync(x =>
                    x.GuildId.Value == Context.Guild.Id && x.DateAdded == date);
                var storeItem = new Shop
                {
                    GuildId = Context.Guild.Id,
                    ItemId = getItem.ItemId,
                    Price = price,
                    SpecialCredit = special
                };
                await db.Shops.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Added {role.Name} to the shop for {price}\nYou can add a description to your item by using `item description <{getItem.ItemId}> <description>`",
                    Color.Green.RawValue);
            }
        }

        [Command("store add", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Add an item to the store")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddStoreItemAsync(string name, string value, int price, bool special = false)
        {
            using (var db = new DbService())
            {
                var date = DateTime.UtcNow;
                var item = new Item
                {
                    Global = false,
                    GuildId = Context.Guild.Id,
                    Name = name,
                    Role = null,
                    Secret = true,
                    SecretValue = value,
                    Unique = true,
                    Value = null,
                    ConsumeOnUse = true,
                    DateAdded = date
                };
                await db.Items.AddAsync(item);
                await db.SaveChangesAsync();

                var getItem = await db.Items.FirstOrDefaultAsync(x =>
                    x.GuildId.Value == Context.Guild.Id && x.DateAdded == date);
                var storeItem = new Shop
                {
                    GuildId = Context.Guild.Id,
                    ItemId = getItem.ItemId,
                    Price = price,
                    SpecialCredit = special
                };
                await db.Shops.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {name} to the shop for {price}", Color.Green.RawValue);
            }
        }

        [Command("store global add", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Summary("Add an item to the store")]
        [RequireOwner]
        public async Task AddGlobalStoreItemAsync(string name, string value, int price)
        {
            using (var db = new DbService())
            {
                var date = DateTime.UtcNow;
                var item = new Item
                {
                    Global = false,
                    GuildId = Context.Guild.Id,
                    Name = name,
                    Role = null,
                    Secret = true,
                    SecretValue = value,
                    Unique = true,
                    Value = null,
                    ConsumeOnUse = true,
                    DateAdded = date
                };
                await db.Items.AddAsync(item);
                await db.SaveChangesAsync();

                var getItem = await db.Items.FirstOrDefaultAsync(x =>
                    x.GuildId.Value == Context.Guild.Id && x.DateAdded == date);
                var storeItem = new StoreGlobal
                {
                    ItemId = getItem.ItemId,
                    Price = price
                };
                await db.StoreGlobals.AddAsync(storeItem);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {name} to the shop for {price}", Color.Green.RawValue);
            }
        }

        private static string SpecialCurrencyResponse(GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency ? $"{CurrencySignEmote(cfg.SpecialCurrencySign)}" : cfg.SpecialCurrencySign;
        }

        private static string GetRegularCurrency(Addons.Database.Tables.Account.Account userdata, GuildConfig cfg)
        {
            return cfg.EmoteCurrency
                ? EmoteCurrencyResponse(userdata.Credit, cfg.CurrencyName, cfg.CurrencySign)
                : RegularCurrencyResponse(userdata.Credit, cfg.CurrencyName, cfg.CurrencySign);
        }

        private static string GetSpecialCurrency(Addons.Database.Tables.Account.Account userdata, GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency
                ? EmoteCurrencyResponse(userdata.CreditSpecial, cfg.SpecialCurrencyName, cfg.SpecialCurrencySign)
                : RegularCurrencyResponse(userdata.CreditSpecial, cfg.SpecialCurrencyName, cfg.SpecialCurrencySign);
        }

        private static string RegularCurrencyResponse(uint credit,
            string name, string sign)
        {
            return $"{name}: {sign}{credit}";
        }

        private static string EmoteCurrencyResponse(uint credit, string name, string sign)
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

    [Group("currency")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class AdminEconomy : InteractiveBase
    {

        [Command("regular name", RunMode = RunMode.Async)]
        [Alias("rn")]
        public async Task SetCurrencyNameAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
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

        [Command("regular symbol", RunMode = RunMode.Async)]
        [Alias("rs")]
        [Priority(1)]
        public async Task SetCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                cfg.EmoteCurrency = true;
                cfg.CurrencySign = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Command("regular symbol", RunMode = RunMode.Async)]
        [Alias("rs")]
        public async Task SetCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
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

        [Command("special name", RunMode = RunMode.Async)]
        [Alias("sn")]
        public async Task SetSpecialCurrencyNameAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
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

        [Command("special symbol", RunMode = RunMode.Async)]
        [Alias("ss")]
        [Priority(1)]
        public async Task SetSpecialCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                cfg.SpecialEmoteCurrency = true;
                cfg.SpecialCurrencySign = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set currency sign to {emote}", Color.Green.RawValue);
            }
        }

        [Command("special symbol", RunMode = RunMode.Async)]
        [Alias("ss")]
        public async Task SetSpecialCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.SpecialEmoteCurrency = false;
                    cfg.SpecialCurrencySign = "$";
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set currency sign back to default `$`", Color.Green.RawValue);
                    return;
                }

                cfg.SpecialEmoteCurrency = false;
                cfg.SpecialCurrencySign = name;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Set regular currency sign to {name}", Color.Green.RawValue);
            }
        }

        private static string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";
        }
    }
}