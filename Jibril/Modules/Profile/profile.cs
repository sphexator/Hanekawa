using Discord.Commands;
using Discord.WebSocket;
using Jibril.Modules.Game.Services;
using Jibril.Modules.Profile.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Profile
{
    public class Profile : ModuleBase<SocketCommandContext>
    {
        [Command("Profile", RunMode = RunMode.Async)]
        [RequiredChannel(360140270605434882)]
        public async Task PostProfile()
        {
            var user = Context.User;

            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();

            var randomString = RandomStringGenerator.StringGenerator();
            var avatar = await DetectBackground.AvatarGenerator(user, randomString);
            var background = await DetectBackground.GetBackground(user, randomString, userData, avatar);
            var finalizeBG = ApplyText.ApplyTextToProfile(background, user, randomString, userData, gameData);

            await Context.Channel.SendFileAsync(finalizeBG);
        }

        [Command("Profile", RunMode = RunMode.Async)]
        [RequiredChannel(360140270605434882)]
        public async Task PostProfile(SocketUser user)
        {

            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var gameData = GameDatabase.GetUserGameStatus(user).FirstOrDefault();

            var randomString = RandomStringGenerator.StringGenerator();
            var avatar = await DetectBackground.AvatarGenerator(user, randomString);
            var background = await DetectBackground.GetBackground(user, randomString, userData, avatar);
            var finalizeBG = ApplyText.ApplyTextToProfile(background, user, randomString, userData, gameData);

            await Context.Channel.SendFileAsync(finalizeBG);
        }
    }
}
