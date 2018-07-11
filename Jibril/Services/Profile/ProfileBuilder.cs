using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Primitives;

namespace Jibril.Services.Profile
{
    public class ProfileBuilder
    {
        public Stream GetProfile(SocketGuildUser user, string url = null)
        {
            var stream = new MemoryStream();

            return stream;
        }

        private async Task<Stream> GetCustomBackground(string url)
        {
            try
            {
                var stream = new MemoryStream();
                using (var client = new HttpClient())
                {
                    var avatar = await client.GetStreamAsync(url);
                    using (var img = Image.Load(avatar))
                    {
                        img.Mutate(x => x.Resize(300, 300));
                        img.Save(stream, new PngEncoder());
                    }
                }

                return stream;
            }
            catch { return await GetBackground(); }
        }

        private async Task<Stream> GetBackground()
        {
            var stream = new MemoryStream();
            var rand = new Random();
            var banner = new DirectoryInfo($"Data/Profile/");
            var images = banner.GetFiles().ToList();
            var randomImage = rand.Next(images.Count);
            using (var img = Image.Load(images[randomImage].DirectoryName))
            {
                img.Mutate(x => x.Resize(300, 300));
                img.Save(stream, new PngEncoder());
            }
            return stream;
        }

        private async Task<Stream> GetAvatar(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var avatar = await client.GetStreamAsync(user.GetAvatar());
                using (var img = Image.Load(avatar))
                {
                    var avi = img.CloneAndConvertToAvatarWithoutApply(new Size(300, 300), 40);
                    avi.Save(stream, new PngEncoder());
                }
            }
            return stream;
        }
    }
}
