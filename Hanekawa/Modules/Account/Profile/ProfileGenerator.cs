using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Services.Level.Util;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Path = SixLabors.Shapes.Path;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace Hanekawa.Modules.Account.Profile
{
    public class ProfileGenerator : IHanaService
    {
        private readonly LevelGenerator _levelGenerator;
        private readonly TextPlacement _text;
        private readonly HttpClient _client;
        private readonly Image<Rgba32> _template;

        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly Font _regular;

        private readonly GraphicsOptions _options = new GraphicsOptions(true);
        private readonly GraphicsOptions _levelOptions = new GraphicsOptions(true, PixelColorBlendingMode.Screen, 1);

        public ProfileGenerator(LevelGenerator calculate, HttpClient client, TextPlacement text)
        {
            _levelGenerator = calculate;
            _client = client;
            _text = text;

            _fonts = new FontCollection();
            _arial = _fonts.Install(@"Data/Fonts/ARIAL.TTF");
            _regular = new Font(_arial, 20, FontStyle.Regular);

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
                CreateProgressBar(img, userdata);
                CreateLevelZone(img, userdata.Level);
                var text = _text.ApplyTextAsync(img, user.GetName().Truncate(25), user.Id, user.Guild.Id, userdata,
                    _levelGenerator);
                var achievement = AddAchievementCircles(img, achieveIcons.Result);
                await Task.WhenAll(text, achievement);
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
                await _text.ApplyTextAsync(img, user.GetName().Truncate(25), user.Id, user.Guild.Id, userdata,
                    _levelGenerator);
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

        private void CreateProgressBar(Image<Rgba32> image, Addons.Database.Tables.Account.Account userdata)
        {
            var percentage = userdata.Exp / (float) _levelGenerator.GetServerLevelRequirement(userdata.Level);
            var numb = (percentage * 100) / 100 * 360 * 2;
            var points = new List<PointF>();
            const double radius = 55;

            for (var i = 0; i < numb; i++)
            {
                var radians = i * Math.PI / 360;

                var x = 200 + radius * Math.Cos(radians - (Math.PI / 2));
                var y = 63 + radius * Math.Sin(radians - (Math.PI / 2));
                points.Add(new PointF((float)x, (float)y));
            }

            image.Mutate(x => x.DrawLines(_options, Rgba32.BlueViolet, 4, points.ToArray()));
        }

        private void CreateLevelZone(Image<Rgba32> image, int level)
        {
            const int heightTop = 107;
            const int heightBot = 124;
            const int width = 199; 

            var padding = DetermineLevelWidth(level);
            image.Mutate(x => x
                //Center
                .FillPolygon(_options, Rgba32.Gray, 
                    new PointF(width - padding, heightTop),
                    new PointF(width + padding, heightTop),
                    new PointF(width + padding, heightBot),
                    new PointF(width - padding, heightBot))
                //Left
                .Fill(_options, Rgba32.Gray, new ComplexPolygon(
                    new Path(
                        new CubicBezierLineSegment(
                            new PointF(width - padding, heightBot), 
                            new PointF(width - padding - 3, heightBot - 4), 
                            new PointF(width - padding - 3, heightTop + 4), 
                            new PointF(width - padding, heightTop)
                            ))))
                //Right
                .Fill(_options, Rgba32.Gray, new ComplexPolygon(
                    new Path(
                        new CubicBezierLineSegment(
                            new PointF(width + padding, heightBot),
                            new PointF(width + padding + 3, heightBot - 4),
                            new PointF(width + padding + 3, heightTop + 4),
                            new PointF(width + padding, heightTop)
                        ))))
                .DrawText($"{level}", _regular, Rgba32.White, new PointF(width - padding, 108))
            );
        }

        private int DetermineLevelWidth(int level)
        {
            if (level > 0 && level < 10) return 5 * 1;
            if (level > 9 && level < 100) return 5 * 2;
            if (level > 99 && level < 1000) return 5 * 3;
            return 100;
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