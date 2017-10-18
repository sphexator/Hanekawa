using Discord.WebSocket;
using Jibril.Services;
using Jibril.Services.Common;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jibril.Modules.Profile.Services
{
    public class ProfileCreator
    {
        public static void PfpCreator(SocketUser user)
        {
            var randomString = RandomStringGenerator.StringGenerator();
            var image = DetectBackground.GetBackground(user, randomString).ToString();
            var applyText = ApplyText.ApplyTextToProfile(image, user, randomString);
        }
    }
}
