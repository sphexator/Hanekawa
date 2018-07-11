using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Primitives;

namespace Jibril.Services.Profile
{
    public class ProfileBuilder
    {
        public async Task<Stream> GetProfileAsync(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            {
                var url = (await db.GetOrCreateUserData(user)).ProfilePic;


            }
            return stream;
        }
        public async Task<Stream> GetProfileAsync(SocketGuildUser user, string url)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var bg = await GetCustomBackgroundAsync(url);
                using (var img = Image.Load(bg))
                {
                    var avatar = Image.Load(await GetAvatarAsync(user));
                    var shipClass = Image.Load($"Data/Profile/Class/{userdata.Class}.png");
                    var template = Image.Load("Data/Profile/Template.png");
                    img.Mutate(x => x
                        .ApplyProfileText(userdata, user)
                        .DrawImage(avatar, new Size(86, 86), new Point(7, 87), GraphicsOptions.Default)
                        .DrawImage(template, new Size(300, 300), new Point(0, 0), GraphicsOptions.Default)
                        .DrawImage(shipClass, new Size(88, 97), new Point(6, 178), GraphicsOptions.Default));
                    img.Save(stream, new PngEncoder());
                }
            }
            return stream;
        }

        private async Task<Stream> GetCustomBackgroundAsync(string url)
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
            catch { return await GetBackgroundAsync(); }
        }

        private async Task<Stream> GetBackgroundAsync()
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

        private async Task<Stream> GetAvatarAsync(SocketGuildUser user)
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
