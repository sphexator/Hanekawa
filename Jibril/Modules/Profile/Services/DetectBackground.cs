using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services;
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
        public static async Task<string> GetBackground(SocketUser user, string randomString)
        {
            var checkBackground = ProfileDB.CheckProfileBG(user);
            var background = $"Data/Images/Profile/Cache/{randomString}background.png";
            if (checkBackground != "o")
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = null;
                response = await httpClient.GetAsync(checkBackground);
                Stream inputStream = await response.Content.ReadAsStreamAsync();
                using (Image<Rgba32> img = Image.Load(inputStream))
                {
                    img.Mutate(x => x
                    .Resize(300, 300));
                    BuildBackground(user, img, randomString);
                    img.Save(background);
                }

                return background;
            }
            else
            {
                Random rand = new Random();
                var randomImage = rand.Next(Images.Profile.Length);
                var filetoLoad = Images.Profile[randomImage];
                using (Image<Rgba32> img = Image.Load(filetoLoad))
                {
                    BuildBackground(user, img, randomString);
                    img.Save(background);
                }
                return background;
            }
        }

        public static Image<Rgba32> BuildBackground(SocketUser user, Image<Rgba32> blargh, string randomString)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var tmptPath = $"Data/Images/Template/profile.png";
            var classPath = $"Data/Images/Profile/ShipClass/{userData.ShipClass}.png";
            var aviS = AvatarGenerator(user, randomString).ToString();
            Image<Rgba32> avi = Image.Load(aviS);
            Image<Rgba32> Shipclass = Image.Load(classPath);
            Image<Rgba32> Template = Image.Load(tmptPath);
            using (Image<Rgba32> img = blargh)
            {
                img.Mutate(x => x
                    .DrawImage(Template, new Size(300, 300), new Point(0, 0), GraphicsOptions.Default)
                    .DrawImage(Shipclass, new Size(300, 300), new Point(6, 178), GraphicsOptions.Default)
                    .DrawImage(avi, new Size(115, 115), new Point(7, 87), GraphicsOptions.Default)
                    );
                return img;
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
