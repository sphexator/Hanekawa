using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Currency;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account
{
    [RequireContext(ContextType.Guild)]
    public class Economy : InteractiveBase
    {
        private readonly CurrencyService _currency;
        public Economy(CurrencyService currency) => _currency = currency;

        [Name("Wallet")]
        [Command("wallet")]
        [Alias("balance", "money")]
        [Summary("Display how much credit you or someone else got")]
        [Remarks("h.wallet")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                if (user == null) user = Context.User as SocketGuildUser;
                var userdata = await db.GetOrCreateUserData(user);
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                var embed = new EmbedBuilder()
                    .CreateDefault($"{cfg.CurrencyName}: {_currency.ToCurrency(cfg, userdata.Credit)}\n" +
                                   $" {cfg.SpecialCurrencyName}: {_currency.ToCurrency(cfg, userdata.CreditSpecial, true)}", Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {IconUrl = user.GetAvatar(), Name = user.GetName()});
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Give")]
        [Command("give", RunMode = RunMode.Async)]
        [Alias("transfer")]
        [Summary("Transfer credit between users")]
        [Remarks("h.give 100 @bob#0000")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task GiveCreditAsync(int amount, SocketGuildUser user)
        {
            if (amount <= 0) return;
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (userdata.Credit < amount)
                {
                    await Context.ReplyAsync($"{Context.User.Mention} doesn't have enough {currencyCfg.CurrencyName}",
                        Color.Red.RawValue);
                    return;
                }

                var receiverData = await db.GetOrCreateUserData(user);

                userdata.Credit = userdata.Credit - amount;
                receiverData.Credit = receiverData.Credit + amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"{Context.User.Mention} transferred {_currency.ToCurrency(currencyCfg, amount)} to {user.Mention}",
                    Color.Green.RawValue);
            }
        }

        [Name("Daily")]
        [Command("daily", RunMode = RunMode.Async)]
        [Summary("Daily credit command, usable once every 18hrs")]
        [Remarks("h.daily")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task DailyAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                    await Context.ReplyAsync($"{Context.User.Mention} daily {currencyCfg.CurrencyName} refresh in {timer.Humanize()}",
                        Color.Red.RawValue);
                    return;
                }

                int reward;
                if (user == null || user == Context.User)
                {
                    user = Context.User as SocketGuildUser;
                    reward = 200;
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.DailyCredit = DateTime.UtcNow;
                    userdata.Credit = userdata.Credit + reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"rewarded {user?.Mention} with {_currency.ToCurrency(currencyCfg, reward)}", Color.Green.RawValue);
                }
                else
                {
                    reward = new Random().Next(200, 300);
                    var userData = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var receiverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    receiverData.Credit = receiverData.Credit + reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"{Context.User.Mention} rewarded {user.Mention} with {_currency.ToCurrency(currencyCfg, reward)}");
                }
            }
        }

        [Name("Richest")]
        [Command("richest", RunMode = RunMode.Async)]
        [Summary("Displays top 10 users on the money leaderboard")]
        [Remarks("h.richest")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);

                var amount = Context.Guild.MemberCount < 50 ? Context.Guild.MemberCount : 50;
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                    .OrderByDescending(account => account.Credit).Take(amount).ToListAsync();
                var rank = 1;
                var pages = new List<string>();
                foreach (var x in users)
                {
                    var user = Context.Guild.GetUser(x.UserId);
                    var name = user == null ? $"User left server ({x.UserId})" : user.Mention;

                    pages.Add($"**Rank: {rank}** - {name}\n" +
                              $"-> {cfg.CurrencyName}: {_currency.ToCurrency(cfg, x.Credit)}");
                    rank++;
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Money leaderboard for {Context.Guild.Name}",
                    10));
            }
        }

        [Name("Reward")]
        [Command("reward", RunMode = RunMode.Async)]
        [Alias("award")]
        [Summary("**Require Manage Server**\nRewards special credit to users (does not remove from yourself)")]
        [Remarks("h.reward 100 @bob#0000")]
        [Ratelimit(1, 1, Measure.Seconds)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RewardCreditAsync(int amount, IGuildUser user)
        {
            if (amount <= 0) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                userdata.CreditSpecial = userdata.CreditSpecial + amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Rewarded {_currency.ToCurrency(cfg, amount, true)} to {user.Mention}",
                    Color.Green.RawValue);
            }
        }
    }
}