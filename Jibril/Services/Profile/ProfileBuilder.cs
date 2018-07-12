using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;
using Jibril.Services.Level.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jibril.Services.Profile
{
    public class ProfileBuilder
    {
        public async Task<MemoryStream> GetProfileAsync(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                Stream bg;
                if (userdata.ProfilePic != null) bg = await GetCustomBackgroundAsync(userdata.ProfilePic);
                else bg = await GetBackgroundAsync();
                bg.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(bg))
                {
                    var avi = await GetAvatarAsync(user);
                    avi.Seek(0, SeekOrigin.Begin);
                    var avatar = Image.Load(avi);
                    avatar.Mutate(x => x.Resize(86, 86));
                    var shipClass = Image.Load($"Data/Profile/Class/{userdata.Class}.png");
                    var template = Image.Load("Data/Profile/Template.png");
                    shipClass.Mutate(x => x.Resize(88, 97));
                    img.Mutate(x => x
                        .DrawImage(GraphicsOptions.Default, template, new Point(0, 0))
                        .DrawImage(GraphicsOptions.Default, avatar, new Point(7, 87))
                        .DrawImage(GraphicsOptions.Default, shipClass, new Point(6, 178))
                        .ApplyProfileText(userdata, user, new Calculate().GetNextLevelRequirement(userdata.Level)));
                    img.Save(stream, new PngEncoder());
                }
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
                    shipClass.Mutate(x => x.Resize(88, 97));
                    img.Mutate(x => x
                        .DrawImage(GraphicsOptions.Default, template, new Point(0, 0))
                        .DrawImage(GraphicsOptions.Default, avatar, new Point(7, 87))
                        .DrawImage(GraphicsOptions.Default, shipClass, new Point(6, 178))
                        .ApplyProfileText(userdata, user, new Calculate().GetNextLevelRequirement(userdata.Level)));
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
            var banner = new DirectoryInfo(@"Data\Profile\Default\");
            var images = banner.GetFiles().ToList();
            var randomImage = images[rand.Next(images.Count)].Name;
            using (var img = Image.Load($"Data/Profile/Default/{randomImage}"))
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
