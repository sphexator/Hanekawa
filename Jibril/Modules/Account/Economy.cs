using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Account
{
    [RequireContext(ContextType.Guild)]
    public class Economy : InteractiveBase
    {
        [Command("wallet")]
        [Alias("balance", "money")]
        [Summary("Display how much credit you got")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.Purple,
                    Description = $"{GetRegularCurrency(userdata, cfg)}\n" +
                                  $"{GetSpecialCurrency(userdata, cfg)}"
                };
                await ReplyAsync(null, false, embed.Build());
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
                var recieverData = await db.GetOrCreateUserData(user);
                if (userdata.Credit < amount)
                {
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have enough credit",
                            Color.Red.RawValue).Build());
                    return;
                }

                userdata.Credit = userdata.Credit - amount;
                recieverData.Credit = recieverData.Credit + amount;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply($"{Context.User.Mention} transferred ${amount} to {user.Mention}",
                        Color.Green.RawValue).Build());
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
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder().Reply($"{Context.User.Mention} daily credit refresh in {timer.Humanize()}",
                            Color.Red.RawValue).Build());
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
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"rewarded {user?.Mention} with ${reward} credit", Color.Green.RawValue).Build());
                }
                else
                {
                    reward = Convert.ToUInt32(new Random().Next(200, 300));
                    var userData = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var recieverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    recieverData.Credit = recieverData.Credit + reward;
                    await db.SaveChangesAsync();
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"{Context.User.Mention} rewarded {user.Mention} with ${reward} credit",
                                Color.Green.RawValue).Build());
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
                var author = new EmbedAuthorBuilder
                {
                    Name = $"Money leaderboard for {Context.Guild.Name}",
                    IconUrl = Context.Guild.IconUrl
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author
                };
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                    .OrderByDescending(account => account.Credit).Take(10).ToListAsync();
                var rank = 1;
                foreach (var x in users)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value = $"<@{x.UserId}> - {cfg.CurrencyName}:{x.Credit}"
                    };
                    embed.AddField(field);
                    rank++;
                }

                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("reward", RunMode = RunMode.Async)]
        [Alias("award")]
        [Summary("Rewards special credit to users (does not remove from yourself)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RewardCreditAsync(uint amount, IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                userdata.CreditSpecial = userdata.CreditSpecial + amount;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Rewarded {SpecialCurrencyResponse(cfg)}{amount} {cfg.SpecialCurrencyName} to {user.Mention}",
                            Color.Green.RawValue).Build());
            }
        }

        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [RequiredChannel]
        public async Task InventoryAsync()
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Inventory is currently disabled", Color.Red.RawValue).Build());
        }

        [Command("shop", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task ShopAsync()
        {
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Shop is currently disabled").Build());
        }

        [Command("buy", RunMode = RunMode.Async)]
        [RequiredChannel]
        public async Task BuyAsync(uint itemId)
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Buy command is currently disabled for rework.", Color.Red.RawValue).Build());
        }

        private static string SpecialCurrencyResponse(GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency ? $"{CurrencySignEmote(cfg.SpecialCurrencySign)}" : cfg.SpecialCurrencySign;
        }

        private static string GetRegularCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return cfg.EmoteCurrency
                ? EmoteCurrencyResponse(userdata.Credit, cfg.CurrencyName, cfg.CurrencySign)
                : RegularCurrencyResponse(userdata.Credit, cfg.CurrencyName, cfg.CurrencySign);
        }

        private static string GetSpecialCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
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
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.CurrencyName = "Credit";
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set regular back to default `Credit`", Color.Green.RawValue)
                            .Build());
                    return;
                }

                cfg.CurrencyName = name;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set regular currency name to {name}", Color.Green.RawValue).Build());
            }
        }

        [Command("regular symbol", RunMode = RunMode.Async)]
        [Alias("rs")]
        [Priority(1)]
        public async Task SetCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.EmoteCurrency = true;
                cfg.CurrencySign = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set currency sign to {emote}", Color.Green.RawValue).Build());
            }
        }

        [Command("regular symbol", RunMode = RunMode.Async)]
        [Alias("rs")]
        public async Task SetCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.EmoteCurrency = false;
                    cfg.CurrencyName = "$";
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set currency sign back to default `$`", Color.Green.RawValue)
                            .Build());
                    return;
                }

                cfg.EmoteCurrency = false;
                cfg.CurrencySign = name;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set regular currency sign to {name}", Color.Green.RawValue).Build());
            }
        }

        [Command("special name", RunMode = RunMode.Async)]
        [Alias("sn")]
        public async Task SetSpecialCurrencyNameAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.SpecialCurrencyName = "Special Credit";
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Set regular back to default `Special Credit`",
                            Color.Green.RawValue).Build());
                    return;
                }

                cfg.SpecialCurrencyName = name;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set regular currency name to {name}", Color.Green.RawValue).Build());
            }
        }

        [Command("special symbol", RunMode = RunMode.Async)]
        [Alias("ss")]
        [Priority(1)]
        public async Task SetSpecialCurrencySignAsync(Emote emote)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.SpecialEmoteCurrency = true;
                cfg.SpecialCurrencySign = ParseEmoteString(emote);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set currency sign to {emote}", Color.Green.RawValue).Build());
            }
        }

        [Command("special symbol", RunMode = RunMode.Async)]
        [Alias("ss")]
        public async Task SetSpecialCurrencySignAsync([Remainder] string name = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.SpecialEmoteCurrency = false;
                    cfg.SpecialCurrencySign = "$";
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set currency sign back to default `$`", Color.Green.RawValue)
                            .Build());
                    return;
                }

                cfg.SpecialEmoteCurrency = false;
                cfg.SpecialCurrencySign = name;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Set regular currency sign to {name}", Color.Green.RawValue).Build());
            }
        }

        private static string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<:{emote.Name}:{emote.Id}>";
        }
    }
}