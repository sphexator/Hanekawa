using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Gamble")]
    [Description("Commands for gambling")]
    [RequireBotGuildPermissions(Permission.SendEmbeds)]
    public class Gamble : HanekawaCommandModule
    {
        [Name("Bet")]
        [Command("bet")]
        [Description("Gamble a certain amount and win 5x. Bet with the bot in an attempt to get same number.")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task<DiscordCommandResult> BetAsync(int bet)
        {
            if (bet <= 0) return null;

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData =
                await db.GetOrCreateEntityAsync<Database.Tables.Account.Account>(Context.GuildId, Context.Author.Id);
            if (userData.Credit == 0) return Reply($"You don't have any credit to gamble with", HanaBaseColor.Bad());
            return await GambleBetAsync(db, userData, await db.GetOrCreateEntityAsync<CurrencyConfig>(Context.GuildId), bet);
        }

        [Name("Roll")]
        [Command("roll")]
        [Description("Gamble a certain amount and win up to 3x. Roll 1-100, above 50 and win")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task<DiscordCommandResult> RollAsync(int bet)
        {
            if (bet <= 0) return null;

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData =
                await db.GetOrCreateEntityAsync<Database.Tables.Account.Account>(Context.GuildId, Context.Author.Id);
            if (userData.Credit == 0) return Reply($"You don't have any credit to gamble with", HanaBaseColor.Bad());
            return await GambleRollAsync(db, userData, await db.GetOrCreateEntityAsync<CurrencyConfig>(Context.GuildId), bet);
        }

        private async Task<DiscordCommandResult> GambleBetAsync(DbContext db, Database.Tables.Account.Account userData, CurrencyConfig cfg, int bet)
        {
            bet = BetAdjust(userData, bet);
            var userRoll = new Random().Next(1, 6);
            var botRoll = new Random().Next(1, 9);
            if (userRoll == botRoll)
            {
                userData.Credit += bet * 5;
                await db.SaveChangesAsync();
                return Reply($"Congratulations! You made a total of **{cfg.ToCurrencyFormat(bet * 2)}** off ${bet}!\n" +
                             $"You rolled: {userRoll} - Bot rolled: {botRoll}", HanaBaseColor.Ok());
            }

            userData.Credit -= bet;
            await db.SaveChangesAsync();
            return Reply(
                $"Sorry, You rolled **{userRoll}** and lost {cfg.ToCurrencyFormat(bet * 2)}\nYou rolled:{userRoll} - Bot rolled: {botRoll}",
                HanaBaseColor.Bad());
        }

        private async Task<DiscordCommandResult> GambleRollAsync(DbContext db, Database.Tables.Account.Account userData, CurrencyConfig cfg, int bet)
        {
            bet = BetAdjust(userData, bet);
            var rolled = new Random().Next(1, 100);
            switch (rolled)
            {
                case >= 95:
                    userData.Credit += bet * 2;
                    await db.SaveChangesAsync();
                    return Reply($"Rolled **{rolled}** and won {cfg.ToCurrencyFormat(bet * 2)}", HanaBaseColor.Ok());
                case >= 65:
                    userData.Credit += bet;
                    await db.SaveChangesAsync();
                    return Reply($"Rolled **{rolled}** and won {cfg.ToCurrencyFormat(bet * 2)}", HanaBaseColor.Ok());
                default:
                    userData.Credit -= bet;
                    await db.SaveChangesAsync();
                    return Reply($"Sorry, You have lost {cfg.ToCurrencyFormat(bet * 2)} Off a roll of **{rolled}**", HanaBaseColor.Bad());
            }
        }

        private static int BetAdjust(Database.Tables.Account.Account userData, int bet)
        {
            if (bet > 25000 && userData.Credit > 25000) return 25000;
            return userData.Credit < bet ? userData.Credit : bet;
        }
    }
}