using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Economy;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        [RequiredChannel]
        public async Task WalletAsync(CachedMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            user ??= Context.Member;
            var userData = await db.GetOrCreateUserData(user);
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            var embed = new LocalEmbedBuilder()
                .Create($"{cfg.CurrencyName}: {_currency.ToCurrency(cfg, userData.Credit)}\n" +
                        $" {cfg.SpecialCurrencyName}: {_currency.ToCurrency(cfg, userData.CreditSpecial, true)}", Context.Colour.Get(Context.Guild.Id.RawValue))
                .WithAuthor(new LocalEmbedAuthorBuilder {IconUrl = user.GetAvatarUrl(), Name = user.DisplayName});
            await ReplyAsync(null, false, embed.Build());
        }

        [Name("Give")]
        [Command("give", "transfer")]
        [Description("Transfer credit between users")]
        [RequiredChannel]
        public async Task GiveCreditAsync(int amount, params CachedMember[] users)
        {
            if (amount <= 0) return;
            if (users.Contains(Context.User)) return;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Member);
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (userData.Credit < amount * users.Length)
            {
                await Context.ReplyAsync($"{Context.User.Mention} doesn't have enough {currencyCfg.CurrencyName}",
                    Color.Red);
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
                Color.Green);
        }

        [Name("Daily")]
        [Command("daily")]
        [Description("Daily credit command, usable once every 18hrs")]
        [RequiredChannel]
        public async Task DailyAsync(CachedMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cooldownCheckAccount = await db.GetOrCreateUserData(Context.Member);
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
            {
                var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                await Context.ReplyAsync(
                    $"{Context.User.Mention} daily {currencyCfg.CurrencyName} refresh in {timer.Humanize(2)}",
                    Color.Red);
                return;
            }

            int reward;
            if (user == null || user == Context.User)
            {
                user = Context.Member;
                reward = 200;
                var userData = await db.GetOrCreateUserData(user);
                userData.DailyCredit = DateTime.UtcNow;
                userData.Credit += reward;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Rewarded {user.Mention} with {_currency.ToCurrency(currencyCfg, reward)}",
                    Color.Green);
            }
            else
            {
                reward = new Random().Next(200, 300);
                var userData = await db.GetOrCreateUserData(Context.Member);
                var receiverData = await db.GetOrCreateUserData(user);
                userData.DailyCredit = DateTime.UtcNow;
                receiverData.Credit += reward;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"{Context.User.Mention} rewarded {user.Mention} with {_currency.ToCurrency(currencyCfg, reward)}", Color.Green);
            }
        }

        [Name("Richest")]
        [Command("richest")]
        [Description("Displays top 10 users on the money leaderboard")]
        [RequiredChannel]
        public async Task LeaderboardAsync(int amount = 50)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);

            amount = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
            var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id.RawValue)
                .OrderByDescending(account => account.Credit).Take(amount).ToListAsync();
            var pages = new List<string>();
            var strBuilder = new StringBuilder();
            for (var i = 0; i < users.Count; i++)
            {
                var x = users[i];
                var user = await Context.Guild.GetOrFetchMemberAsync(x.UserId);
                if (user == null)
                {
                    x.Active = false;
                    continue;
                }
                strBuilder.AppendLine($"**Rank: {i + 1}** - {user.Mention}");
                strBuilder.Append($"-> {cfg.CurrencyName}: {_currency.ToCurrency(cfg, x.Credit)}");
                pages.Add(strBuilder.ToString());
                strBuilder.Clear();
            }

            await db.SaveChangesAsync();
            await Context.PaginatedReply(pages, Context.Guild, $"Money leaderboard for {Context.Guild.Name}", pageSize: 10);
        }

        [Name("Reward")]
        [Command("reward", "award")]
        [Description("Rewards special credit to users (does not remove from yourself)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RewardCreditAsync(int amount, params CachedMember[] users)
        {
            if (amount <= 0) return;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
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
                $"Rewarded {_currency.ToCurrency(cfg, amount, true)} to:\n {strBuilder}", Color.Green);
        }
    }
}