using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account.Gamble
{
    [Name("Gamble")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Gamble : HanekawaCommandModule
    {
        [Name("Bet")]
        [Command("bet")]
        [Description("Gamble a certain amount and win 5x. Bet with the bot in an attempt to get same number.")]
        [RequiredChannel]
        public async Task BetAsync(int bet)
        {
            if (bet <= 0) return;
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Member);
            if (userData.Credit == 0)
            {
                await Context.ReplyAsync($"{Context.User.Mention} doesn't have any credit to gamble with",
                    Color.Red);
                return;
            }

            await Context.ReplyAsync(await GambleBetAsync(db, Context, userData, bet));
        }

        [Name("Roll")]
        [Command("roll")]
        [Description("Gamble a certain amount and win up to 3x. Roll 1-100, above 50 and win")]
        [RequiredChannel]
        public async Task RollAsync(int bet)
        {
            if (bet <= 0) return;
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Member);
            if (userData.Credit == 0)
            {
                await Context.ReplyAsync($"{Context.User.Mention} doesn't have any credit to gamble with",
                    Color.Red);
                return;
            }

            await Context.ReplyAsync(await GambleRollAsync(db, Context, userData, bet));
        }

        private async Task<LocalEmbedBuilder> GambleBetAsync(DbService db, DiscordCommandContext context,
            Database.Tables.Account.Account userData, int bet)
        {
            if (userData.Credit < bet) bet = BetAdjust(userData);
            if (bet > 5000) bet = BetAdjust();
            var userRoll = new Random().Next(1, 6);
            var botRoll = new Random().Next(1, 9);
            if (userRoll == botRoll)
            {
                userData.Credit += bet * 5;
                await db.SaveChangesAsync();
                return new LocalEmbedBuilder().Create(
                    $"Congratulations {context.User.Mention}!, You made a total of **${bet * 5}** off ${bet}!\n" +
                    $"You rolled: {userRoll} - Bot rolled: {botRoll}", Color.Green);
            }

            userData.Credit -= bet;
            await db.SaveChangesAsync();
            return new LocalEmbedBuilder().Create(
                $"Sorry **{context.User.Mention}**, You rolled **{userRoll}** and lost ${bet}\n " +
                $"You rolled:{userRoll} - Bot rolled: {botRoll}", Color.Red);
        }

        private async Task<LocalEmbedBuilder> GambleRollAsync(DbService db, DiscordCommandContext context,
            Database.Tables.Account.Account userData, int bet)
        {
            if (userData.Credit < bet) bet = BetAdjust(userData);
            if (bet > 5000) bet = BetAdjust();

            var rolled = new Random().Next(1, 100);

            if (rolled >= 90)
            {
                userData.Credit += bet * 2;
                await db.SaveChangesAsync();
                return new LocalEmbedBuilder().Create(
                    $"{context.User.Mention} rolled **{rolled}** and won ${bet * 2}",
                    Color.Green);
            }

            if (rolled >= 50)
            {
                userData.Credit += bet;
                await db.SaveChangesAsync();
                return new LocalEmbedBuilder().Create($"{context.User.Mention} rolled **{rolled}** and won ${bet}",
                    Color.Green);
            }

            userData.Credit -= bet;
            await db.SaveChangesAsync();
            return new LocalEmbedBuilder().Create(
                $"Sorry **{context.User.Mention}**, You have lost ${bet} Off a roll of **{rolled}**",
                Color.Red);
        }

        private static int BetAdjust(Database.Tables.Account.Account userData) =>
            userData.Credit >= 25000 ? 25000 : userData.Credit;

        private static int BetAdjust() => 5000;
    }
}