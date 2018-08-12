using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;

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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }
                await ReplyAsync(null, false, (await GambleBetAsync(Context, db, userdata, bet)).Build());
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
                        new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }
                var bet = userdata.Credit;
                await ReplyAsync(null, false, (await GambleBetAsync(Context, db, userdata, bet, true)).Build());
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
                        new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }
                await ReplyAsync(null, false, (await GambleRollAsync(Context, db, userdata, bet)).Build());
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
                        new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have any credit to gamble with",
                            Color.Red.RawValue).Build());
                    return;
                }
                var bet = userdata.Credit;
                await ReplyAsync(null, false, (await GambleRollAsync(Context, db, userdata, bet, true)).Build());
            }
        }

        private static async Task<EmbedBuilder> GambleBetAsync(SocketCommandContext context, DbContext db, Services.Entities.Tables.Account userdata, uint bet, bool allin = false)
        {
            if (userdata.Credit < bet) bet = BetAdjust(userdata);
            if (bet > 5000 && !allin) bet = BetAdjust();
            var userRoll = new Random().Next(1, 6);
            var botRoll = new Random().Next(1, 9);
            if (userRoll == botRoll)
            {
                userdata.Credit = userdata.Credit + (bet * 5);
                await db.SaveChangesAsync();
                return new EmbedBuilder().Reply($"Congratulations {context.User.Mention}!, You made a total of **${bet * 5}** off ${bet}!\n" +
                                                $"You rolled: {userRoll} - Bot rolled: {botRoll}", Color.Green.RawValue);
            }
            userdata.Credit = userdata.Credit - bet;
            await db.SaveChangesAsync();
            return new EmbedBuilder().Reply($"Sorry **{context.User.Mention}**, You rolled **{userRoll}** and lost ${bet}\n " +
                                            $"You rolled:{userRoll} - Bot rolled: {botRoll}", Color.Red.RawValue);
        }

        private static async Task<EmbedBuilder> GambleRollAsync(SocketCommandContext context, DbContext db, Services.Entities.Tables.Account userdata, uint bet, bool allin = false)
        {
            if (userdata.Credit < bet) bet = BetAdjust(userdata);
            if (bet > 5000 && !allin) bet = BetAdjust();

            var rolled = new Random().Next(1, 100);

            if (rolled >= 90)
            {
                userdata.Credit = userdata.Credit + (bet * 2);
                await db.SaveChangesAsync();
                return new EmbedBuilder().Reply($"{context.User.Mention} rolled **{rolled}** and won ${bet * 2}", Color.Green.RawValue);
            }

            if (rolled >= 50)
            {
                userdata.Credit = userdata.Credit + bet;
                await db.SaveChangesAsync();
                return new EmbedBuilder().Reply($"{context.User.Mention} rolled **{rolled}** and won ${bet}", Color.Green.RawValue);
            }

            userdata.Credit = userdata.Credit - bet;
            await db.SaveChangesAsync();
            return new EmbedBuilder().Reply($"Sorry **{context.User.Mention}**, You have lost ${bet} Off a roll of **{rolled}**", Color.Red.RawValue);
        }

        private static uint BetAdjust(Services.Entities.Tables.Account userdata)
        {
            return userdata.Credit;
        }

        private static uint BetAdjust()
        {
            return 5000;
        }
    }
}