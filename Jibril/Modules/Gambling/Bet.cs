using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Gambling.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Gambling
{
    public class Bet : ModuleBase<SocketCommandContext>
    {
        [Command("bet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds, false, false)]
        public async Task HardBet(int amount)
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(user);
                var bet = BetAdjust.Adjust(amount);
                if (amount <= 0)
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you don't have enough credit",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                if (userData.Tokens < bet)
                {
                    var embed = EmbedGenerator.DefaultEmbed(
                        $":thinking: {user.Mention} don't have enough money for that kind of bet.", Colours.FailColour);
                    await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    return;
                }

                if (userData.Tokens >= bet)
                {
                    var rand = new Random();
                    var rand2 = new Random();

                    var userRoll = rand2.Next(1, 6);
                    var rolled = rand.Next(1, 9);

                    if (userRoll == rolled)
                    {
                        var award = bet * 5;
                        userData.Tokens = userData.Tokens + Convert.ToUInt32(award);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Congratulations {user.Mention}!, You made a total of ${award} off ${bet}!\n" +
                            $"You rolled:{userRoll} - Bot rolled: {rolled}", Colours.OkColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        userData.Tokens = userData.Tokens - Convert.ToUInt32(bet);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Sorry **{user.Mention}**, You rolled **{userRoll}** and lost ${bet}\n " +
                            $"You rolled:{userRoll} - Bot rolled: {rolled}", Colours.FailColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("bet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds, false, false)]
        public async Task HardBet(string bet)
        {
            if (!(bet.Equals("all", StringComparison.InvariantCultureIgnoreCase))) return;
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(user);
                if (userData == null) return;
                var amount = userData.Tokens;

                if (userData.Tokens != 0)
                {
                    var rand = new Random();
                    var rand2 = new Random();

                    var userRoll = rand2.Next(1, 6);
                    var rolled = rand.Next(1, 9);

                    if (userRoll == rolled)
                    {
                        var cward = Convert.ToInt32(amount);
                        var award = cward * 5;
                        userData.Tokens = userData.Tokens + Convert.ToUInt32(award);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Congratulations {user.Mention}!, You made a total of ${award} off ${amount}!\n" +
                            $"You rolled:{userRoll} - Bot rolled: {rolled}", Colours.OkColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        userData.Tokens = userData.Tokens - Convert.ToUInt32(bet);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Sorry **{user.Mention}**, You rolled **{userRoll}** and lost ${amount}\n " +
                            $"You rolled:{userRoll} - Bot rolled: {rolled}", Colours.FailColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("roll")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds, false, false)]
        public async Task BetRoll(int amount)
        {
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
                if (amount <= 0)
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you don't have enough credit",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                    return;
                }

                var bet = BetAdjust.Adjust(amount);

                if (userdata.Tokens >= bet)
                {
                    var rand = new Random();
                    var rolled = rand.Next(1, 100);

                    if (rolled >= 90)
                    {
                        var award = bet * 2;
                        userdata.Tokens = userdata.Tokens + Convert.ToUInt32(award);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} rolled **{rolled}** and won ${award}",
                            Colours.OkColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else if (rolled >= 50)
                    {
                        var award = bet;
                        userdata.Tokens = userdata.Tokens + Convert.ToUInt32(award);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} rolled **{rolled}** and won ${award}",
                            Colours.OkColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        userdata.Tokens = userdata.Tokens - Convert.ToUInt32(bet);
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Sorry **{user.Mention}**, You have lost ${bet} Off a roll of **{rolled}**",
                            Colours.FailColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("roll")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds, false, false)]
        public async Task BetRoll(string bet)
        {
            if (bet.Equals("allin", StringComparison.InvariantCultureIgnoreCase) != true) return;
            var user = Context.User;
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
                if (userdata == null) return;
                var amount = userdata.Tokens;

                if (userdata.Tokens != 0)
                {
                    var rand = new Random();
                    var rolled = rand.Next(1, 100);

                    if (rolled >= 90)
                    {
                        var award = amount * 2;
                        userdata.Tokens = userdata.Tokens + award;
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} rolled **{rolled}** and won ${award}",
                            Colours.OkColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else if (rolled >= 50)
                    {
                        var award = amount;
                        userdata.Tokens = userdata.Tokens + award;
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} rolled **{rolled}** and won ${award}",
                            Colours.OkColour);
                        await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        userdata.Tokens = userdata.Tokens - amount;
                        await db.SaveChangesAsync();
                        var embed = EmbedGenerator.DefaultEmbed(
                            $"Sorry **{user.Mention}**, You have lost ${bet} Off a roll of **{rolled}**",
                            Colours.FailColour);
                        await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}