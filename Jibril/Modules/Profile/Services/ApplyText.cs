using Discord.WebSocket;
using Jibril.Services;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Profile.Services
{
    public class ApplyText
    {
        public static string ApplyTextToProfile(string filepath, SocketUser user, string randomString)
        {
            var userData = DatabaseService.UserData(user);
            using(Image<Rgba32> img = Image.Load(filepath))
            {

            }
            return "1231231231";
        }
    }
}
