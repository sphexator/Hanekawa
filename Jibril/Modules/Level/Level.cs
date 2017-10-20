using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using Jibril.Services.Level.Services;
using Jibril.Services;
using Jibril.Data.Variables;
using Jibril.Preconditions;

namespace Jibril.Modules.Level
{
    public class Level : ModuleBase<SocketCommandContext>
    {
        [Command("rank")]
        [Alias("Rank")]
        [RequiredChannel(339383206669320192)]
        public async Task LevelChecker()
        {
            var user = Context.User as SocketGuildUser;
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var thumbnailurl = user.GetAvatarUrl();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var auth = new EmbedAuthorBuilder()
            {
                Name = user.Username,
                IconUrl = thumbnailurl,
            };
            var embed = new EmbedBuilder()
            {
                Color = new Color(Colours.DefaultColour),
                Author = auth
            };

            EmbedFieldBuilder EmbedField = new EmbedFieldBuilder();
            EmbedField.WithIsInline(true);
            EmbedField.WithName("Level");
            EmbedField.WithValue($"{userData.Level}");

            EmbedFieldBuilder EmbedField2 = new EmbedFieldBuilder();
            EmbedField2.WithIsInline(true);
            EmbedField2.WithName("Exp");
            EmbedField2.WithValue($"{userData.Xp}/{xpToLevelUp}");

            embed.AddField(EmbedField);
            embed.AddField(EmbedField2);

            await Context.Channel.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("rank")]
        [Alias("Rank")]
        [RequiredChannel(339383206669320192)]
        public async Task LevelCheckerOther(SocketGuildUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var xpToLevelUp = Calculate.CalculateNextLevel(userData.Level);
            var thumbnailurl = user.GetAvatarUrl();

            var auth = new EmbedAuthorBuilder()
            {
                Name = user.Username,
                IconUrl = thumbnailurl,
            };
            var embed = new EmbedBuilder()
            {
                Color = new Color(Colours.DefaultColour),
                Author = auth
            };

            EmbedFieldBuilder EmbedField = new EmbedFieldBuilder();
            EmbedField.WithIsInline(true);
            EmbedField.WithName("Level");
            EmbedField.WithValue($"{userData.Level}");

            EmbedFieldBuilder EmbedField2 = new EmbedFieldBuilder();
            EmbedField2.WithIsInline(true);
            EmbedField2.WithName("Exp");
            EmbedField2.WithValue($"{userData.Xp}/{xpToLevelUp}");

            embed.AddField(EmbedField);
            embed.AddField(EmbedField2);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("top10")]
        [Alias("top")]
        [Remarks("Shows top10 on the leaderboard")]
        [RequiredChannel(339383206669320192)]
        public async Task Leaderboard()
        {
            EmbedBuilder embed = new EmbedBuilder();
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
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }
    }
}
