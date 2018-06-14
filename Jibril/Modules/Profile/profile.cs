using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
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
        public async Task PostProfile(SocketGuildUser user = null)
        {
            using (var db = new hanekawaContext())
            {
                if(user == null) user = Context.User as SocketGuildUser;

                var userData = await db.GetOrCreateUserData(user);
                var gameData = await db.GetOrCreateShipGame(user);

                var randomString = RandomStringGenerator.StringGenerator();
                var avatar = await DetectBackground.AvatarGenerator(user, randomString);
                var background = await DetectBackground.GetBackground(user, randomString, userData, avatar);
                var finalizeBg = ApplyText.ApplyTextToProfile(background, user, randomString, userData, gameData);

                await Context.Channel.SendFileAsync(finalizeBg);
                RemoveImage.RemoveSavedProfile();
            }
        }
    }
}