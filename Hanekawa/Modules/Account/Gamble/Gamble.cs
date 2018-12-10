using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Gamble
{
    public class Gamble : InteractiveBase
    {
        [Command("bet", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task BetAsync(uint bet)
        {
            if (bet == 0) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (userdata.Credit == 0)
                {
                    await Context.ReplyAsync($"{Context.User.Mention} doesn't have any credit to gamble with",
                        Color.Red.RawValue);
                    return;
                }

                await ReplyAsync(null, false, (await GambleBetAsync(db, Context, userdata, bet)).Build());
            }
        }

        [Command("bet", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task BetAllAsync(string amount)
        {
            if (!amount.Equals("all")) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);

                if (userdata.Credit == 0)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().CreateDefault($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }

                var bet = userdata.Credit;
                await ReplyAsync(null, false, (await GambleBetAsync(db, Context, userdata, bet, true)).Build());
            }
        }

        [Command("roll", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task RollAsync(uint bet)
        {
            if (bet == 0) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (userdata.Credit == 0)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().CreateDefault($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }

                await ReplyAsync(null, false, (await GambleRollAsync(db, Context, userdata, bet)).Build());
            }
        }

        [Command("roll", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task RollAllAsync(string amount)
        {
            if (!amount.Equals("all")) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (userdata.Credit == 0)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().CreateDefault($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }

                var bet = userdata.Credit;
                await ReplyAsync(null, false, (await GambleRollAsync(db, Context, userdata, bet, true)).Build());
            }
        }

        private async Task<EmbedBuilder> GambleBetAsync(DbService db, SocketCommandContext context,
            Addons.Database.Tables.Account.Account userdata, uint bet, bool allin = false)
        {
            if (userdata.Credit < bet) bet = BetAdjust(userdata);
            if (bet > 5000 && !allin) bet = BetAdjust();
            var userRoll = new Random().Next(1, 6);
            var botRoll = new Random().Next(1, 9);
            if (userRoll == botRoll)
            {
                userdata.Credit = userdata.Credit + bet * 5;
                await db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault(
                    $"Congratulations {context.User.Mention}!, You made a total of **${bet * 5}** off ${bet}!\n" +
                    $"You rolled: {userRoll} - Bot rolled: {botRoll}", Color.Green.RawValue);
            }

            userdata.Credit = userdata.Credit - bet;
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault(
                $"Sorry **{context.User.Mention}**, You rolled **{userRoll}** and lost ${bet}\n " +
                $"You rolled:{userRoll} - Bot rolled: {botRoll}", Color.Red.RawValue);
        }

        private async Task<EmbedBuilder> GambleRollAsync(DbService db, SocketCommandContext context,
            Addons.Database.Tables.Account.Account userdata, uint bet, bool allin = false)
        {
            if (userdata.Credit < bet) bet = BetAdjust(userdata);
            if (bet > 5000 && !allin) bet = BetAdjust();

            var rolled = new Random().Next(1, 100);

            if (rolled >= 90)
            {
                userdata.Credit = userdata.Credit + bet * 2;
                await db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault(
                    $"{context.User.Mention} rolled **{rolled}** and won ${bet * 2}",
                    Color.Green.RawValue);
            }

            if (rolled >= 50)
            {
                userdata.Credit = userdata.Credit + bet;
                await db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault($"{context.User.Mention} rolled **{rolled}** and won ${bet}",
                    Color.Green.RawValue);
            }

            userdata.Credit = userdata.Credit - bet;
            await db.SaveChangesAsync();
            return new EmbedBuilder().CreateDefault(
                $"Sorry **{context.User.Mention}**, You have lost ${bet} Off a roll of **{rolled}**",
                Color.Red.RawValue);
        }

        private static uint BetAdjust(Addons.Database.Tables.Account.Account userdata)
        {
            return userdata.Credit >= 25000 ? 25000 : userdata.Credit;
        }

        private static uint BetAdjust()
        {
            return 5000;
        }
    }
}