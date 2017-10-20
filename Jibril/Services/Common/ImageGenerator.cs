using Discord.WebSocket;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Common
{
    public class ImageGenerator
    {
        public static void RandomBackground()
        {

        }

        public static async Task<string> AvatarGenerator(SocketUser user, string randomString)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            var aviPath = $"Data/Welcome/Cache/Avatar/{randomString}.jpg";
            try
            {
                response = await httpClient.GetAsync(user.GetAvatarUrl());
            }
            catch
            {
                response = await httpClient.GetAsync("https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            }
            Stream inputStream = await response.Content.ReadAsStreamAsync();
            using (var img = Image.Load(inputStream))
            {
                img.Save(aviPath);
                return aviPath;
            }
        }
    }
}
