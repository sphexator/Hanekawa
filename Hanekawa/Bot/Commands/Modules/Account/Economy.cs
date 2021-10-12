using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Economy")]
    [Description("Commands for user economy")]
    [RequireBotGuildPermissions(Permission.SendEmbeds | Permission.SendMessages)]
    public class Economy : HanekawaCommandModule
    {
        [Name("Wallet")]
        [Command("wallet", "balance", "money")]
        [Description("Display how much credit you or someone else got")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> WalletAsync(IMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            user ??= Context.Author;
            var userData = await db.GetOrCreateUserData(user);
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);

            var embed = new LocalEmbed
            {
                Author = new LocalEmbedAuthor {IconUrl = user.GetAvatarUrl(), Name = user.DisplayName()},
                Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Description = $"{cfg.CurrencyName}: {cfg.ToCurrencyFormat(userData.Credit)}\n" +
                              $" {cfg.SpecialCurrencyName}: {cfg.ToCurrencyFormat(userData.CreditSpecial, true)}"
            };

            return Reply(embed);
        }

        [Name("Give")]
        [Command("give", "transfer")]
        [Description("Transfer credit between users")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> GiveCreditAsync(int amount, params IMember[] users)
        {
            if (amount <= 0) return null;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Author);
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (userData.Credit < amount * users.Length)
            {
                return Reply($"{Context.Author.Mention} doesn't have enough {currencyCfg.CurrencyName}",
                    HanaBaseColor.Bad());
            }

            var str = new StringBuilder();
            foreach (var user in users)
            {
                if (user == Context.Author) continue;
                var receiverData = await db.GetOrCreateUserData(user);

                userData.Credit -= amount;
                receiverData.Credit += amount;
                str.AppendLine($"{user.Mention}");
            }

            await db.SaveChangesAsync();
            return Reply(
                $"{Context.Author.Mention} transferred {currencyCfg.ToCurrencyFormat(amount)} to:\n{str}",
                HanaBaseColor.Ok());
        }

        [Name("Daily")]
        [Command("daily")]
        [Description("Daily credit command, usable once every 18hrs")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> DailyAsync(IMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Author);
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            if (userData.DailyCredit.Date.AddDays(1) > DateTime.UtcNow)
            {
                var timer = userData.DailyCredit.Date.AddDays(1) - DateTime.UtcNow;
                return Reply(
                    $"{Context.Author.Mention} daily {currencyCfg.CurrencyName} refresh in {timer.Humanize(2)}\n" +
                    "Dailies reset at midnight UTC!",
                    HanaBaseColor.Bad());
            }

            int reward;
            if (user == null || user == Context.Author)
            {
                user = Context.Author;
                reward = 200;
                userData.DailyCredit = DateTime.UtcNow.Date;
                userData.Credit += reward;
                await db.SaveChangesAsync();
                return Reply(
                    $"Rewarded {user.Mention} with {currencyCfg.ToCurrencyFormat(reward)}",
                    HanaBaseColor.Ok());
            }

            reward = new Random().Next(200, 300);
            var receiverData = await db.GetOrCreateUserData(user);
            userData.DailyCredit = DateTime.UtcNow.Date;
            receiverData.Credit += reward;
            await db.SaveChangesAsync();
            return Reply(
                $"{Context.Author.Mention} rewarded {user.Mention} with {currencyCfg.ToCurrencyFormat(reward)}",
                HanaBaseColor.Ok());
        }

        [Name("Richest")]
        [Command("richest")]
        [Description("Displays top 10 users on the money leaderboard")]
        [RequiredChannel]
        public async Task<DiscordMenuCommandResult> LeaderboardAsync(int amount = 50)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);

            amount = Context.Guild.MemberCount < amount ? Context.Guild.MemberCount : amount;
            var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id)
                .OrderByDescending(account => account.Credit).Take(amount).ToListAsync();
            var pages = new List<string>();
            var str = new StringBuilder();
            for (var i = 0; i < users.Count; i++)
            {
                var x = users[i];
                var user = await Context.Guild.GetOrFetchMemberAsync(x.UserId);
                if (user == null)
                {
                    x.Active = false;
                    continue;
                }

                str.AppendLine($"**Rank: {i + 1}** - {user.Mention}");
                str.Append($"-> {cfg.CurrencyName}: {cfg.ToCurrencyFormat(x.Credit)}");
                pages.Add(str.ToString());
                str.Clear();
            }

            await db.SaveChangesAsync();
            return Pages(pages.Pagination(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"Money leaderboard for {Context.Guild.Name}", 10));
        }

        [Name("Reward")]
        [Command("reward", "award")]
        [Description("Rewards special credit to users (does not remove from yourself)")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> RewardCreditAsync(int amount, params CachedMember[] users)
        {
            if (amount <= 0) return null;
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var str = new StringBuilder();
            var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
            foreach (var user in users)
            {
                var userData = await db.GetOrCreateUserData(user);
                userData.CreditSpecial += amount;
                str.AppendLine($"{user.Mention}");
            }

            await db.SaveChangesAsync();
            return Reply(
                $"Rewarded {cfg.ToCurrencyFormat(amount, true)} to:\n {str}", HanaBaseColor.Ok());
        }

        [Group("Currency")]
        [Name("Currency Admin")]
        [Description("Currency management")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public class EconomySettings : Economy
        {
            [Name("Regular Currency Name")]
            [Command("name")]
            [Description("Change the name of regular currency (default: credit)")]
            public async Task<DiscordCommandResult> SetRegularNameAsync([Remainder] string name = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.CurrencyName = "Credit";
                    await db.SaveChangesAsync();
                    return Reply("Set regular back to default `Credit`", HanaBaseColor.Ok());
                }

                cfg.CurrencyName = name;
                await db.SaveChangesAsync();
                return Reply($"Set regular currency name to {name}", HanaBaseColor.Ok());
            }

            [Name("Special Currency Name")]
            [Command("sname")]
            [Description("Change the name of special currency (default: special credit)")]
            public async Task<DiscordCommandResult> SetSpecialNameAsync([Remainder] string name = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (name.IsNullOrWhiteSpace())
                {
                    cfg.SpecialCurrencyName = "Special Credit";
                    await db.SaveChangesAsync();
                    return Reply("Set regular back to default `Special Credit`",
                        HanaBaseColor.Ok());
                }

                cfg.SpecialCurrencyName = name;
                await db.SaveChangesAsync();
                return Reply($"Set regular currency name to {name}", HanaBaseColor.Ok());
            }

            [Name("Regular Currency Symbol")]
            [Command("symbol")]
            [Description("Change the symbol of regular currency (default: $)")]
            public async Task<DiscordCommandResult> SetRegularSymbolAsync(IGuildEmoji emote)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.EmoteCurrency = true;
                cfg.CurrencySign = emote.GetMessageFormat();
                await db.SaveChangesAsync();
                return Reply($"Set regular currency sign to {emote}", HanaBaseColor.Ok());
            }

            [Name("Regular Currency Symbol")]
            [Command("symbol")]
            [Description("Change the symbol of regular currency (default: $)")]
            public async Task<DiscordCommandResult> SetRegularSymbolAsync([Remainder] string symbol)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (symbol.IsNullOrWhiteSpace()) symbol = "$";

                cfg.EmoteCurrency = false;
                cfg.CurrencySign = symbol;
                await db.SaveChangesAsync();
                return Reply($"Set regular currency sign to {symbol}", Color.Green);
            }

            [Name("Special Currency Symbol")]
            [Command("ssymbol")]
            [Description("Change the symbol of special currency (default: $)")]
            public async Task<DiscordCommandResult> SetSpecialSymbolAsync(IGuildEmoji emote)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                cfg.SpecialEmoteCurrency = true;
                cfg.SpecialCurrencySign = emote.GetMessageFormat();
                await db.SaveChangesAsync();
                return Reply($"Set special currency sign to {emote}", Color.Green);
            }

            [Name("Special Currency Symbol")]
            [Command("ssymbol")]
            [Description("Change the symbol of special currency (default: $)")]
            public async Task<DiscordCommandResult> SetSpecialSymbolAsync([Remainder] string symbol)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateCurrencyConfigAsync(Context.Guild);
                if (symbol.IsNullOrWhiteSpace()) symbol = "$";

                cfg.SpecialEmoteCurrency = false;
                cfg.SpecialCurrencySign = symbol;
                await db.SaveChangesAsync();
                return Reply($"Set special currency sign to {symbol}", Color.Green);
            }
        }
    }
}