using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Gambling.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;
using Jibril.Services.Level.Services;

namespace Jibril.Modules.Level
{
    public class Level : ModuleBase<SocketCommandContext>
    {
        [Command("rank")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LevelChecker()
        {
            var user = Context.User as SocketGuildUser;
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var embed = EmbedGenerator.AuthorEmbed("", $"{user.Username}", Colours.DefaultColour, user);
            //var dbAmount = DatabaseService.CheckDbAmount();
            //var rank = DatabaseService.CheckRank(userData);

            var level = new EmbedFieldBuilder
            {
                Name = "Level",
                IsInline = true,
                Value = $"{userData.Level}"
            };
            var exp = new EmbedFieldBuilder
            {
                Name = "Exp",
                IsInline = true,
                Value = $"{userData.Xp}/{xpToLevelUp}"
            };
            //var ranking = new EmbedFieldBuilder
            //{
            //    Name = "Rank",
            //    IsInline = true,
            //    Value = $"{rank}/{dbAmount}"
            //};

            embed.AddField(level);
            embed.AddField(exp);
            //embed.AddField(ranking);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LevelCheckerOther(SocketGuildUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var embed = EmbedGenerator.AuthorEmbed("", $"{user.Username}", Colours.DefaultColour, user);
            //var dbAmount = DatabaseService.CheckDbAmount();
            //var rank = DatabaseService.CheckRank(userData);

            var level = new EmbedFieldBuilder
            {
                Name = "Level",
                IsInline = true,
                Value = $"{userData.Level}"
            };
            var exp = new EmbedFieldBuilder
            {
                Name = "Exp",
                IsInline = true,
                Value = $"{userData.Xp}/{xpToLevelUp}"
            };
            //var ranking = new EmbedFieldBuilder
            //{
            //    Name = "Rank",
            //    IsInline = true,
            //    Value = $"{rank}/{dbAmount}"
            //};

            embed.AddField(level);
            embed.AddField(exp);
            //embed.AddField(ranking);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("top10")]
        [Alias("top")]
        [Remarks("Shows top10 on the leaderboard")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task Leaderboard()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(Colours.DefaultColour));
            embed.Title = "Leaderboard";
            var result = LevelDatabase.GetLeaderBoard().ToList();
            for (var i = 0; i < 10; i++)
            {
                var c = result[i];
                var rank = i + 1;
                embed.AddField(y =>
                {
                    y.Name = $"Rank {rank}";
                    y.Value = $"<@!{c.UserId}> | Level:{c.Level} - Total Exp:{c.Total_xp}";
                    y.IsInline = false;
                });
            }

            await ReplyAsync(" ", false, embed.Build());
        }

        [Command("give", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task GiveCredit(int amount, IGuildUser user)
        {
            var userData = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userData?.Tokens < amount)
            {
                var failres = EmbedGenerator.DefaultEmbed("Not enough credit to give.", Colours.FailColour);
                await ReplyAsync("", false, failres.Build());
                return;
            }
            if (userData?.Level < 20)
            {
                var failres = EmbedGenerator.DefaultEmbed("Not hight enough level to give people credit.", Colours.FailColour);
                await ReplyAsync("", false, failres.Build());
                return;
            }
            GambleDB.RemoveCredit(Context.User, amount);
            GambleDB.AddCredit(user, amount);
            var embed = EmbedGenerator.DefaultEmbed(
                $"{Context.User.Username} has given {user.Username} {amount} credit", Colours.DefaultColour);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("daily", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Daily()
        {
            var user = Context.User;
            var result = DatabaseService.CheckUser(user);
            if (!result.Any()) DatabaseService.EnterUser(user);
            var userData = DatabaseService.UserData(user).FirstOrDefault();

            var now = DateTime.Now;
            var daily = userData.Daily;
            var difference = DateTime.Compare(daily, now);

            if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                daily.AddDays(1) <= now && difference < 0 || difference >= 0)
            {
                LevelDatabase.ChangeDaily(user);
                if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                    daily.AddDays(1) <= now && difference <= 0 || difference >= 0)
                {
                    var tokens = 200;
                    GambleDB.AddCredit(user, tokens);
                    await ReplyAsync($"You received your daily ${tokens}!");
                }
            }
            else
            {
                var diff = now - daily;
                var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                await ReplyAsync($"Your credits refresh in {di}!");
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Daily(IGuildUser rewardUser)
        {
            var user = Context.User;
            var result = DatabaseService.CheckUser(user);
            if (!result.Any()) DatabaseService.EnterUser(user);
            var userData = DatabaseService.UserData(user).FirstOrDefault();

            var now = DateTime.Now;
            var daily = userData.Daily;
            var difference = DateTime.Compare(daily, now);

            if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                daily.AddDays(1) <= now && difference < 0 || difference >= 0)
            {
                LevelDatabase.ChangeDaily(user);
                if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                    daily.AddDays(1) <= now && difference <= 0 || difference >= 0)
                {
                    var rand = new Random();
                    var tokens = rand.Next(200, 400);
                    GambleDB.AddCredit(rewardUser, tokens);
                    await ReplyAsync($"You received your daily ${tokens}!");
                }
            }
            else
            {
                var diff = now - daily;
                var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                await ReplyAsync($"Your credits refresh in {di}!");
            }
        }

        [Command("rep", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Rep(IGuildUser user)
        {
            if (user == Context.User) return;

            var result = DatabaseService.CheckUser(user);
            if (!result.Any()) DatabaseService.EnterUser(user);
            var userData = DatabaseService.UserData(Context.User).FirstOrDefault();

            var now = DateTime.UtcNow;
            var daily = userData.Repcd;
            var difference = DateTime.Compare(daily, now);

            if (userData.Repcd.ToString() == "0001-01-01 00:00:00" ||
                daily.AddDays(1) <= now && difference < 0 || difference >= 0)
            {
                LevelDatabase.ChangeDaily(user);
                if (userData.Repcd.ToString() == "0001-01-01 00:00:00" ||
                    daily.AddDays(1) <= now && difference <= 0 || difference >= 0)
                {
                    GambleDB.AddRep(user);
                    await ReplyAsync($"{Context.User.Mention} gave a reputation point to {user.Mention}!");
                }
            }
            else
            {
                var diff = now - daily;
                var di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);
                await ReplyAsync($"You can reward another rep in {di}!");
            }
        }
    }
}