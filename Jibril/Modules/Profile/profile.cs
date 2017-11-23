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
    public class Profile : ModuleBase<SocketCommandContext>
    {
        [Command("Profile", RunMode = RunMode.Async)]
        [Summary("Displays your server profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task PostProfile()
        {
            var user = Context.User;
            DbRequirement(user);

            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();

            var randomString = RandomStringGenerator.StringGenerator();
            var avatar = await DetectBackground.AvatarGenerator(user, randomString);
            var background = await DetectBackground.GetBackground(user, randomString, userData, avatar);
            var finalizeBG = ApplyText.ApplyTextToProfile(background, user, randomString, userData, gameData);

            await Context.Channel.SendFileAsync(finalizeBG);
            RemoveImage.RemoveSavedProfile();
        }

        [Command("Profile", RunMode = RunMode.Async)]
        [Summary("Displays your server profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task PostProfile(SocketUser user)
        {
            DbRequirement(user);

            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();

            var randomString = RandomStringGenerator.StringGenerator();
            var avatar = await DetectBackground.AvatarGenerator(user, randomString);
            var background = await DetectBackground.GetBackground(user, randomString, userData, avatar);
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