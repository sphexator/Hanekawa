using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Services.Level.Util;

namespace Hanekawa.Services.Profile
{
    public class ProfileBuilder : IHanaService
    {
        private readonly LevelGenerator _levelGenerator;

        public ProfileBuilder(LevelGenerator levelGenerator) => _levelGenerator = levelGenerator;

        public async Task<MemoryStream> GetProfileAsync(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                Image<Rgba32> bg;
                if (userdata.ProfilePic != null) bg = await GetCustomBackgroundAsync(userdata.ProfilePic);
                else bg = GetBackgroundAsync();
                using (var img = bg)
                {
                    var avi = await GetAvatarAsync(user);
                    avi.Seek(0, SeekOrigin.Begin);
                    var avatar = Image.Load(avi);
                    avatar.Mutate(x => x.Resize(86, 86));
                    var shipClass = Image.Load("Data/Profile/Class/Battleship.png");
                    var template = Image.Load("Data/Profile/Template.png");
                    shipClass.Mutate(x => x.Resize(88, 97));
                    img.Mutate(x => x
                        .DrawImage(GraphicsOptions.Default, template, new Point(0, 0))
                        .DrawImage(GraphicsOptions.Default, avatar, new Point(7, 87))
                        .DrawImage(GraphicsOptions.Default, shipClass, new Point(6, 178))
                        .ApplyProfileText(userdata, user, (uint) _levelGenerator.GetServerLevelRequirement(userdata.Level)));
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
                using (var img = bg)
                {
                    var avatar = Image.Load(await GetAvatarAsync(user));
                    var shipClass = Image.Load("Data/Profile/Class/Battleship.png");
                    var template = Image.Load("Data/Profile/Template.png");
                    shipClass.Mutate(x => x.Resize(88, 97));
                    img.Mutate(x => x
                        .DrawImage(GraphicsOptions.Default, template, new Point(0, 0))
                        .DrawImage(GraphicsOptions.Default, avatar, new Point(7, 87))
                        .DrawImage(GraphicsOptions.Default, shipClass, new Point(6, 178))
                        .ApplyProfileText(userdata, user, (uint) _levelGenerator.GetServerLevelRequirement(userdata.Level)));
                    img.Save(stream, new PngEncoder());
                }
            }

            return stream;
        }

        private async Task<Image<Rgba32>> GetCustomBackgroundAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var background = await client.GetStreamAsync(url);
                    using (var img = Image.Load(background))
                    {
                        img.Mutate(x => x.Resize(300, 300));
                        return img.Clone();
                    }
                }
            }
            catch
            {
                return GetBackgroundAsync();
            }
        }

        private Image<Rgba32> GetBackgroundAsync()
        {
            var rand = new Random();
            var banner = new DirectoryInfo("Data/Profile/Default/");
            var images = banner.GetFiles().ToList();
            var randomImage = images[rand.Next(images.Count)].Name;
            using (var img = Image.Load($"Data/Profile/Default/{randomImage}"))
            {
                img.Mutate(x => x.Resize(300, 300));
                return img.Clone();
            }
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