using Discord.WebSocket;
using Jibril.Data.Variables;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jibril.Modules.Profile.Services
{
    public static class DetectBackground
    {
        public static async Task<string> GetBackground(SocketUser user, string randomString, Exp userData,
            string avatar)
        {
            var background = $"Data/Images/Profile/Cache/{randomString}background.png";
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(userData.Profilepic);
                var inputStream = await response.Content.ReadAsStreamAsync();
                using (var img = Image.Load(inputStream))
                {
                    img.Mutate(x => x
                        .Resize(300, 300));
                    img.Save(background);
                }
                var fbg = BuildBackground(user, background, randomString, userData, avatar);
                return fbg;
            }
            catch
            {
                var rand = new Random();
                var randomImage = rand.Next(Images.Profile.Length);
                var filetoLoad = Images.Profile[randomImage];
                using (var img = Image.Load(filetoLoad))
                {
                    img.Save(background);
                }
                var fbg = BuildBackground(user, background, randomString, userData, avatar);
                return fbg;
            }
        }
        private static string BuildBackground(SocketUser user, string background, string randomString,
            Exp userData,
            string aviS)
        {
            const string tmptPath = "Data/Images/Profile/Template/profile.png";
            var classPath = $"Data/Images/Profile/ShipClass/{userData.ShipClass}.png";
            var avi = Image.Load(aviS);
            var shipclass = Image.Load(classPath);
            var template = Image.Load(tmptPath);
            using (var img = Image.Load(background))
            {
                img.Mutate(x => x
                    .DrawImage(template, new Size(300, 300), new Point(0, 0), GraphicsOptions.Default)
                    .DrawImage(shipclass, new Size(88, 97), new Point(6, 178), GraphicsOptions.Default)
                    .DrawImage(avi, new Size(86, 86), new Point(7, 87), GraphicsOptions.Default)
                );
                img.Save(background);
                return background;
            }
        }

        public static async Task<string> AvatarGenerator(SocketUser user, string randomString)
        {
            var filePath = $"Data/Images/Profile/Cache/{randomString}avi.png";
            var httpclient = new HttpClient();
            HttpResponseMessage response;

            try
            {
                response = await httpclient.GetAsync(user.GetAvatarUrl());
            }
            catch
            {
                response = await httpclient.GetAsync(
                    "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            }
            var inputStream = await response.Content.ReadAsStreamAsync();
            using (var img = Image.Load(inputStream))
            {
                img.Save(filePath);
                return filePath;
            }
        }

        public static async Task<string> PreviewBackgroundTask(SocketUser user, string randomString, Exp userData,
            string avatar, string background)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response = null;
            response = await httpClient.GetAsync(background);
            var inputStream = await response.Content.ReadAsStreamAsync();
            using (var img = Image.Load(inputStream))
            {
                img.Mutate(x => x
                    .Resize(300, 300));
                img.Save(background);
            }

            var fbg = BuildBackground(user, background, randomString, userData, avatar);
            return fbg;
        }
    }
}