using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Modules.Account.Profile
{
    public class ProfileGenerator
    {
        public async Task<Stream> Create(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            using (var client = new HttpClient())
            using (var img = new Image<Rgba32>(400, 400))
            {
                var gpOptions = new GraphicsOptions(false);
                var aviOptions = new GraphicsOptions(true);
                var userdata = await db.GetOrCreateUserData(user);
                var background = GetBackground(db, client, userdata);
                var templateBg = GetTemplateBackground();
                var pfpCircle = GetPfpCircle();
                var circle = GetCircle();
                await Task.WhenAll(circle, pfpCircle, templateBg, background);
                var avi = await GetAvatar(user, client);

                img.Mutate(x => x
                    .DrawImage(background.Result, 1)
                    .DrawImage(gpOptions, templateBg.Result, new Point(0, 0))
                    .DrawImage(aviOptions, avi, new Point(149, 8))
                    .DrawImage(gpOptions, pfpCircle.Result, new Point(149, 8)));
                img.Mutate(x => x.ApplyTextAsync(user.Username, user.Id, userdata).GetAwaiter().GetResult());
                img.Mutate(x => x.ApplyAchievementCircles(circle.Result));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private static async Task<Image<Rgba32>> GetBackground(DbService db, HttpClient client, Addons.Database.Tables.Account.Account userdata)
        {
            Stream stream;
            if (!userdata.ProfilePic.IsNullOrWhiteSpace())
            {
                stream = await client.GetStreamAsync(userdata.ProfilePic);
            }
            else
            {
                stream = await GetDefaultBackground(client, db);
            }

            using (var img = Image.Load(stream))
            {
                img.Mutate(x => x.Resize(400, 400));
                return img.Clone();
            }
        }

        private static async Task<Stream> GetDefaultBackground(HttpClient client, DbService db)
        {
            var img = await db.Backgrounds.OrderBy(r => Guid.NewGuid()).Take(1).FirstAsync();
            return await client.GetStreamAsync(img.BackgroundUrl);
        }

        private static async Task<Image<Rgba32>> GetTemplateBackground()
        {
            using (var stream = new MemoryStream())
            {
                new FileStream(@"Data\Profile\Template\ProfileTemplate.png", FileMode.Open).CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(stream))
                {
                    return img.Clone();
                }
            }
        }

        private static async Task<Image<Rgba32>> GetPfpCircle()
        {
            using (var stream = new MemoryStream())
            {
                new FileStream(@"Data\Profile\Template\ProfileCircle.png", FileMode.Open).CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(stream))
                {
                    return img.Clone();
                }
            }
        }

        private static async Task<Image<Rgba32>> GetCircle()
        {
            using (var stream = new MemoryStream())
            {
                new FileStream(@"Data\Profile\Template\AchievementCircle.png", FileMode.Open).CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(stream))
                {
                    return img.Clone();
                }
            }
        }

        private static async Task<Image<Rgba32>> GetAvatar(IUser user, HttpClient client)
        {
            var avi = await client.GetStreamAsync(user.GetAvatar());
            using (var img = Image.Load(avi))
            {
                return img.CloneAndConvertToAvatarWithoutApply(new Size(110, 110), 61).Clone();
            }
        }

    }
}
