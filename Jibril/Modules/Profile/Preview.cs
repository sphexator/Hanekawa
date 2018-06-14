using System.Linq;
using System.Threading.Tasks;
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
    public class Preview : ModuleBase<SocketCommandContext>
    {
        [Command("preview", RunMode = RunMode.Async)]
        [Summary("Displays your server profile")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task PostProfile([Remainder] string bg = null)
        {
            using (var db = new hanekawaContext())
            {
                var userData = await db.GetOrCreateUserData(Context.User);
                var gameData = await db.GetOrCreateShipGame(Context.User);

                var randomString = RandomStringGenerator.StringGenerator();
                var avatar = await DetectBackground.AvatarGenerator(Context.User, randomString);
                var background = await DetectBackground.PreviewBackgroundTask(Context.User, randomString, userData, avatar, bg);
                var finalizeBg = ApplyText.ApplyTextToProfile(background, Context.User, randomString, userData, gameData);

                await Context.Channel.SendFileAsync(finalizeBg);
                RemoveImage.RemoveSavedProfile();
            }
        }
    }
}