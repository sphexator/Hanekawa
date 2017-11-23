using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Modules.Game.Services;
using Jibril.Modules.Profile.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Profile
{
    public class Preview : ModuleBase<SocketCommandContext>
    {
        [Command("preview", RunMode = RunMode.Async)]
        [Summary("Displays your server profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task PostProfile([Remainder]string bg = null)
        {
            var user = Context.User;
            DbRequirement(user);

            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();

            var randomString = RandomStringGenerator.StringGenerator();
            var avatar = await DetectBackground.AvatarGenerator(user, randomString);
            var background = await DetectBackground.PreviewBackgroundTask(user, randomString, userData, avatar, bg);
            var finalizeBg = ApplyText.ApplyTextToProfile(background, user, randomString, userData, gameData);

            await Context.Channel.SendFileAsync(finalizeBg);
            RemoveImage.RemoveSavedProfile();
        }

        private static void DbRequirement(SocketUser user)
        {
            var check = GameDatabase.GameCheckExistingUser(user);
            if (check == null)
                GameDatabase.AddNPCDefault(user, 100);
        }
    }
}