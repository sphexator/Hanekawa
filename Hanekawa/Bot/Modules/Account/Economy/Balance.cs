﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Economy
{
    public partial class Economy
    {
        private readonly CurrencyService _currency;
        public Economy(CurrencyService currency) => _currency = currency;

        [Name("Wallet")]
        [Command("wallet", "balance", "money")]
        [Description("Display how much credit you or someone else got")]
        [Remarks("wallet")]
        [RequiredChannel]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                if (user == null) user = Context.User;
                var userData = await db.GetOrCreateUserData(user);
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                var embed = new EmbedBuilder()
                    .CreateDefault($"{cfg.CurrencyName}: {_currency.ToCurrency(cfg, userData.Credit)}\n" +
                                   $" {cfg.SpecialCurrencyName}: {_currency.ToCurrency(cfg, userData.CreditSpecial, true)}",
                        Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {IconUrl = user.GetAvatar(), Name = user.GetName()});
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Give")]
        [Command("give", "transfer")]
        [Description("Transfer credit between users")]
        [Remarks("give 100 @bob#0000")]
        [RequiredChannel]
        public async Task GiveCreditAsync(int amount, params SocketGuildUser[] users)
        {
            if (amount <= 0) return;
            if (users.Contains(Context.User)) return;
            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(Context.User);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (userData.Credit < amount * users.Length)
                {
                    await Context.ReplyAsync($"{Context.User.Mention} doesn't have enough {currencyCfg.CurrencyName}",
                        Color.Red.RawValue);
                    return;
                }

                var strBuilder = new StringBuilder();
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var receiverData = await db.GetOrCreateUserData(user);

                    userData.Credit -= amount;
                    receiverData.Credit += amount;
                    strBuilder.AppendLine($"{user.Mention}");
                }
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"{Context.User.Mention} transferred {_currency.ToCurrency(currencyCfg, amount)} to:\n{strBuilder}",
                    Color.Green.RawValue);
            }
        }

        [Name("Daily")]
        [Command("daily")]
        [Description("Daily credit command, usable once every 18hrs")]
        [Remarks("daily")]
        [RequiredChannel]
        public async Task DailyAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                    await Context.ReplyAsync(
                        $"{Context.User.Mention} daily {currencyCfg.CurrencyName} refresh in {timer.Humanize()}",
                        Color.Red.RawValue);
                    return;
                }

                int reward;
                if (user == null || user == Context.User)
                {
                    user = Context.User;
                    reward = 200;
                    var userData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    userData.Credit += reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync(
                        $"Rewarded {user.Mention} with {_currency.ToCurrency(currencyCfg, reward)}",
                        Color.Green.RawValue);
                }
                else
                {
                    reward = new Random().Next(200, 300);
                    var userData = await db.GetOrCreateUserData(Context.User);
                    var receiverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    receiverData.Credit += reward;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync(
                        $"{Context.User.Mention} rewarded {user.Mention} with {_currency.ToCurrency(currencyCfg, reward)}");
                }
            }
        }

        [Name("Richest")]
        [Command("richest")]
        [Description("Displays top 10 users on the money leaderboard")]
        [Remarks("richest")]
        [RequiredChannel]
        public async Task LeaderboardAsync(int amount = 50)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);

                amount = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                    .OrderByDescending(account => account.Credit).Take(amount).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < users.Count; i++)
                {
                    var x = users[i];
                    var user = Context.Guild.GetUser(x.UserId);
                    var name = user == null ? $"User left server ({x.UserId})" : user.Mention;

                    pages.Add($"**Rank: {i + 1}** - {name}\n" +
                              $"-> {cfg.CurrencyName}: {_currency.ToCurrency(cfg, x.Credit)}");
                }
                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild,
                    $"Money leaderboard for {Context.Guild.Name}", null, 10));
            }
        }

        [Name("Reward")]
        [Command("reward", "award")]
        [Description("Rewards special credit to users (does not remove from yourself)")]
        [Remarks("reward 100 @bob#0000")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RewardCreditAsync(int amount, params SocketGuildUser[] users)
        {
            if (amount <= 0) return;
            using (var db = new DbService())
            {
                var strBuilder = new StringBuilder();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var userData = await db.GetOrCreateUserData(user);
                    userData.CreditSpecial += amount;
                    strBuilder.AppendLine($"{user.Mention}");
                }
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Rewarded {_currency.ToCurrency(cfg, amount, true)} to:\n {strBuilder}",
                    Color.Green.RawValue);
            }
        }
    }
}