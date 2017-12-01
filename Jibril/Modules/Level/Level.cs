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
using Jibril.Services.Level.Services;

namespace Jibril.Modules.Level
{
    public class Level : ModuleBase<SocketCommandContext>
    {
        [Command("rank")]
        [Alias("Rank")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1,2, Measure.Seconds)]
        public async Task LevelChecker()
        {
            var user = Context.User as SocketGuildUser;
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var thumbnailurl = user.GetAvatarUrl();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var auth = new EmbedAuthorBuilder
            {
                Name = user.Username,
                IconUrl = thumbnailurl
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour),
                Author = auth
            };

            var EmbedField = new EmbedFieldBuilder();
            EmbedField.WithIsInline(true);
            EmbedField.WithName("Level");
            EmbedField.WithValue($"{userData.Level}");

            var EmbedField2 = new EmbedFieldBuilder();
            EmbedField2.WithIsInline(true);
            EmbedField2.WithName("Exp");
            EmbedField2.WithValue($"{userData.Xp}/{xpToLevelUp}");

            embed.AddField(EmbedField); 
            embed.AddField(EmbedField2);

            await Context.Channel.SendMessageAsync(" ", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LevelCheckerOther(SocketGuildUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var thumbnailurl = user.GetAvatarUrl();

            var auth = new EmbedAuthorBuilder
            {
                Name = user.Username,
                IconUrl = thumbnailurl
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour),
                Author = auth
            };

            var EmbedField = new EmbedFieldBuilder();
            EmbedField.WithIsInline(true);
            EmbedField.WithName("Level");
            EmbedField.WithValue($"{userData.Level}");

            var EmbedField2 = new EmbedFieldBuilder();
            EmbedField2.WithIsInline(true);
            EmbedField2.WithName("Exp");
            EmbedField2.WithValue($"{userData.Xp}/{xpToLevelUp}");

            embed.AddField(EmbedField);
            embed.AddField(EmbedField2);
            await ReplyAsync(" ", false, embed.Build());
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
            await ReplyAsync(" ", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("daily", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Daily()
        {
            var user = Context.User;
            var result = DatabaseService.CheckUser(user);
            if (result.Count() <= 0) DatabaseService.EnterUser(user);
            var userData = DatabaseService.UserData(user).FirstOrDefault();

            var now = DateTime.Now;
            var daily = userData.Daily;
            var difference = DateTime.Compare(daily, now);

            if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                daily.DayOfYear < now.DayOfYear && difference < 0 || difference >= 0)
            {
                LevelDatabase.ChangeDaily(user);
                if (userData.Daily.ToString() == "0001-01-01 00:00:00" ||
                    daily.DayOfYear < now.DayOfYear && difference < 0 || difference >= 0)
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

        [Command("ping", RunMode = RunMode.Async)]
        [RequiredChannel(1231231231)]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        [Ratelimit(1, 20, Measure.Hours)]
        public async Task PingTask()
        {
            var msg = await ReplyAsync("@here");
            await msg.DeleteAsync();
        }
    }
}