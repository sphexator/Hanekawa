using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services;
using Jibril.Services.Level.Lists;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Profile.Services
{
    public class DetectBackground
    {
        public static async Task<string> GetBackground(SocketUser user, string randomString, UserData userData, string avatar)
        {
            var checkBackground = DatabaseService.UserData(user).FirstOrDefault();
            var background = $"Data/Images/Profile/Cache/{randomString}background.png";
            if (checkBackground.Profilepic != "o")
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = null;
                response = await httpClient.GetAsync(checkBackground.Profilepic);
                Stream inputStream = await response.Content.ReadAsStreamAsync();
                using (Image<Rgba32> img = Image.Load(inputStream))
                {
                    img.Mutate(x => x
                    .Resize(300, 300));
                    img.Save(background);
                }
                var fbg = BuildBackground(user, background, randomString, userData, avatar);
                return fbg;
            }
            else
            {
                Random rand = new Random();
                var randomImage = rand.Next(Images.Profile.Length);
                var filetoLoad = Images.Profile[randomImage];
                using (Image<Rgba32> img = Image.Load(filetoLoad))
                {
                    img.Save(background);
                }
                var fbg = BuildBackground(user, background, randomString, userData, avatar);
                return fbg;
            }
        }

        public static string BuildBackground(SocketUser user, string background, string randomString, UserData userData, string aviS)
        {
            var tmptPath = $"Data/Images/Profile/Template/profile.png";
            var classPath = $"Data/Images/Profile/ShipClass/{userData.ShipClass}.png";
            Image<Rgba32> avi = Image.Load(aviS);
            Image<Rgba32> Shipclass = Image.Load(classPath);
            Image<Rgba32> Template = Image.Load(tmptPath);
            using (Image<Rgba32> img = Image.Load(background))
            {
                img.Mutate(x => x
                    .DrawImage(Template, new Size(300, 300), new Point(0, 0), GraphicsOptions.Default)
                    .DrawImage(Shipclass, new Size(88, 97), new Point(6, 178), GraphicsOptions.Default)
                    .DrawImage(avi, new Size(86, 86), new Point(7, 87), GraphicsOptions.Default)
                    );
                img.Save(background);
                return background;
            }
        }

        public static async Task<String> AvatarGenerator(SocketUser user, string randomString)
        {
            var filePath = $"Data/Images/Profile/Cache/{randomString}avi.png";
            HttpClient httpclient = new HttpClient();
            HttpResponseMessage response = null;

            try
            {
                response = await httpclient.GetAsync(user.GetAvatarUrl());
            }
            catch
            {
                response = await httpclient.GetAsync("https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            }
            Stream inputStream = await response.Content.ReadAsStreamAsync();
            using (Image<Rgba32> img = Image.Load(inputStream))
            {
                img.Save(filePath);
                return filePath;
            }
        }
    }
}
