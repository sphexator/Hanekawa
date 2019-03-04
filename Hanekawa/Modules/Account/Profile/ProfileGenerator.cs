using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Services.Level.Util;
using Humanizer;
using SixLabors.Fonts;
using SixLabors.Shapes;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Modules.Account.Profile
{
    public class ProfileGenerator : IHanaService
    {
        private readonly LevelGenerator _levelGenerator;
        private readonly HttpClient _client;
        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly Font _regular;
        private readonly Font _name;
        private readonly Image<Rgba32> _template;
        
        private readonly TextGraphicsOptions _nameOptions = 
            new TextGraphicsOptions {HorizontalAlignment = HorizontalAlignment.Center};
        private readonly TextGraphicsOptions _rightOptions = 
            new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Center };
        private readonly TextGraphicsOptions _leftOptions = 
            new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Center };
        private readonly GraphicsOptions _options = new GraphicsOptions(true);

        public ProfileGenerator(LevelGenerator calculate, HttpClient client)
        {
            _levelGenerator = calculate;
            _client = client;
            _fonts = new FontCollection();
            _arial = _fonts.Install(@"Data/Fonts/ARIAL.TTF");
            _regular = new Font(_arial, 20, FontStyle.Regular);
            _name = new Font(_arial, 32, FontStyle.Regular);
            _template = Image.Load("Data/Profile/Template/ProfileTemplate.png");
        }

        public async Task<Stream> Create(SocketGuildUser user, Stream stream)
        {
            using (var db = new DbService())
            using (var img = new Image<Rgba32>(400, 400))
            {
                var userdata = await db.GetOrCreateUserData(user);
                var background = GetBackground(db, _client, userdata);
                var achieveIcons = GetAchievementIcons(user, db);
                var avi = GetAvatar(user, _client);
                await Task.WhenAll(background, achieveIcons, avi);
                img.Mutate(x => x
                    .DrawImage(background.Result, 1)
                    .DrawImage(_template, new Point(0, 0), _options)
                    .DrawImage(avi.Result, new Point(145, 8), _options)
                    .Fill(_options, Rgba32.Gray, new EllipsePolygon(200, 63, 55).GenerateOutline(4)));
                img.Mutate(x =>
                    x.ApplyTextAsync(user.Username.Truncate(25), user.Id, user.Guild.Id, userdata, _levelGenerator).GetAwaiter()
                        .GetResult());
                await AddAchievementCircles(img, achieveIcons.Result);
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        public async Task<Stream> Preview(SocketGuildUser user, string url)
        {
            var stream = new MemoryStream();
            using (var db = new DbService())
            using (var client = new HttpClient())
            using (var img = new Image<Rgba32>(400, 400))
            {
                var gpOptions = new GraphicsOptions(false);
                var aviOptions = new GraphicsOptions(true);
                var userdata = await db.GetOrCreateUserData(user);
                var background = Image.Load(await client.GetStreamAsync(url));
                var achievIcons = await GetAchievementIcons(user, db);
                var avi = await GetAvatar(user, client);

                img.Mutate(x => x
                    .DrawImage(background, 1)
                    .DrawImage(_template, new Point(0, 0), gpOptions)
                    .DrawImage(avi, new Point(149, 8), aviOptions)
                    .Fill(_options, Rgba32.Gray, new EllipsePolygon(149, 8, 50).GenerateOutline(1)));
                img.Mutate(x =>
                    x.ApplyTextAsync(user.Username.Truncate(25), user.Id, user.Guild.Id, userdata, _levelGenerator).GetAwaiter()
                        .GetResult());
                await AddAchievementCircles(img, achievIcons);
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private async Task<Image<Rgba32>> GetBackground(DbService db, HttpClient client,
            Addons.Database.Tables.Account.Account userdata)
        {
            Stream stream;
            if (!userdata.ProfilePic.IsNullOrWhiteSpace())
                stream = await client.GetStreamAsync(userdata.ProfilePic);
            else
                stream = await GetDefaultBackground(client, db);

            using (var img = Image.Load(stream))
            {
                img.Mutate(x => x.Resize(400, 400));
                return img.Clone();
            }
        }

        private async Task<Stream> GetDefaultBackground(HttpClient client, DbService db)
        {
            var img = await db.Backgrounds.OrderBy(r => new Random().Next()).Take(1).FirstAsync();
            return await client.GetStreamAsync(img.BackgroundUrl);
        }

        private async Task<Image<Rgba32>> GetAvatar(IUser user, HttpClient client)
        {
            using (var avi = await client.GetStreamAsync(user.GetAvatar()))
            using (var img = Image.Load(avi))
            {
                return img.CloneAndConvertToAvatarWithoutApply(new Size(110, 110), 61).Clone();
            }
        }

        private async Task<IEnumerable<Image<Rgba32>>> GetAchievementIcons(IGuildUser user, DbService db)
        {
            var achievements = new Dictionary<int, Image<Rgba32>>();
            var unlocks = await db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
            var list = await UnlockToAchieve(db, unlocks);
            foreach (var x in list)
            {
                if (achievements.Count != 0 && achievements.ContainsKey(x.AchievementNameId)) continue;
                var achiev = await db.Achievements.FindAsync(x.AchievementId);
                var nAchiev = await db.AchievementNames.FindAsync(achiev.AchievementNameId);
                if (nAchiev.Stackable)
                {
                    var image = await GetHighestStackable(x, list, db);
                    achievements.TryAdd(nAchiev.AchievementNameId, image);
                }
                else
                {
                    var image = await GetIcon(achiev);
                    achievements.TryAdd(nAchiev.AchievementNameId, image);
                }
            }

            var result = new List<Image<Rgba32>>();
            foreach (var x in achievements) result.Add(x.Value);
            return result;
        }

        private async Task<List<AchievementMeta>> UnlockToAchieve(DbService db,
            IEnumerable<AchievementUnlock> list)
        {
            var result = new List<AchievementMeta>();
            foreach (var x in list) result.Add(await db.Achievements.FindAsync(x.AchievementId));

            return result;
        }

        private async Task<Image<Rgba32>> GetIcon(AchievementMeta achiev)
        {
            var dir = Directory.CreateDirectory($"Data/Achievement/Image/{achiev.AchievementNameId}/");
            var file = dir.GetFiles($"{achiev.AchievementId}.png");
            if (file.Length == 0) return await OnlineIcon(achiev);
            using (var image =
                Image.Load($"Data/Achievement/Image/{achiev.AchievementNameId}/{achiev.AchievementId}.png"))
            {
                return image.CloneAndConvertToAvatarWithoutApply(new Size(80, 80), 50).Clone();
            }
        }

        private async Task<Image<Rgba32>> OnlineIcon(AchievementMeta achiev)
        {
            using (var image = Image.Load(await _client.GetStreamAsync(achiev.ImageUrl)))
            {
                DownloadIcon(image, achiev);
                return image.CloneAndConvertToAvatarWithoutApply(new Size(80, 80), 50).Clone();
            }
        }

        private void DownloadIcon(Image<Rgba32> image, AchievementMeta achiev)
        {
            image.Save($"Data/Achievement/Image/{achiev.AchievementNameId}/{achiev.AchievementId}.png");
        }

        private async Task<Image<Rgba32>> GetHighestStackable(AchievementMeta achiev,
            IEnumerable<AchievementMeta> list, DbService db)
        {
            var achievementUnlocks = list.ToList();
            var max = achievementUnlocks.Where(x => x.AchievementNameId == achiev.AchievementNameId)
                .Max(x => x.Requirement);
            var result = achievementUnlocks.First(x =>
                x.AchievementNameId == achiev.AchievementNameId && x.Requirement == max);
            return await GetIcon(result);
        }

        private Task AddAchievementCircles(Image<Rgba32> img, IEnumerable<Image<Rgba32>> icons)
        {
            const int height = 346;
            const int width = 61;
            var images = icons.ToList();
            if (images.Count > 4)
            {
                const int spacerW = 45;
                const int spacerH = 45;
                var amount = 0;
                for (var i = 0; i < 2; i++)
                for (var j = 0; j < 8; j++)
                {
                    if (amount >= images.Count) continue;
                    var icon = images[amount];
                    icon.Mutate(y => y.Resize(39, 39));
                    img.Mutate(x => x.DrawImage(icon,
                        new Point(width + spacerW * j, height + spacerH * i), _options));
                    amount++;
                }

                for (var i = 0; i < 2; i++)
                for (var j = 0; j < 8; j++)
                    img.Mutate(z => z
                        .Fill(_options, Rgba32.Gray, new EllipsePolygon(width + spacerW * j, height + spacerH * i, 39).GenerateOutline(1)));
            }
            else
            {
                const int spacerW = 93;
                var amount = 0;
                foreach (var x in images)
                {
                    img.Mutate(z => z.DrawImage(x, new Point(width + spacerW * amount, height), _options));
                    amount++;
                }

                for (var i = 0; i < 4; i++)
                    img.Mutate(z => z
                        .Fill(_options, Rgba32.Gray, new EllipsePolygon(width + spacerW * i, height, 39).GenerateOutline(1)));
            }

            return Task.CompletedTask;
        }
    }
}