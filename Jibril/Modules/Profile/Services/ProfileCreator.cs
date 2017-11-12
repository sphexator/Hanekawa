using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services.Level.Lists;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileCreator
    {
        public static void PfpCreator(SocketUser user, string randomString, GameStatus gameData, UserData userData)
        {
            /*
            var image = DetectBackground.GetBackground(user, randomString).ToString();
            var applyText = ApplyText.ApplyTextToProfile(image, user, randomString, userData, gameData);
            return applyText;
            */
        }
    }
}