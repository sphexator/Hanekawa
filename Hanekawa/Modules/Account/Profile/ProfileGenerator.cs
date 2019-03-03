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
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Modules.Account.Profile
{
    public class ProfileGenerator : IHanaService
    {
        private readonly LevelGenerator _levelGenerator;

        public ProfileGenerator(LevelGenerator calculate) => _levelGenerator = calculate;

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
                var background = await GetBackground(db, client, userdata);
                var templateBg = GetTemplateBackground();
                var pfpCircle = GetPfpCircle();
                var circle = GetCircle();
                var achieveIcons = await GetAchievementIcons(user, db);
                var avi = await GetAvatar(user, client);

                img.Mutate(x => x
                    .DrawImage(background, 1)
                    .DrawImage(templateBg, new Point(0, 0), gpOptions)
                    .DrawImage(avi, new Point(149, 8), aviOptions)
                    .DrawImage(pfpCircle, new Point(149, 8), gpOptions));
                img.Mutate(x =>
                    x.ApplyTextAsync(user.Username.Truncate(25), user.Id, user.Guild.Id, userdata, _levelGenerator).GetAwaiter()
                        .GetResult());
                img.Mutate(x => x.ApplyAchievementCircles(circle, achieveIcons));
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
                var templateBg = GetTemplateBackground();
                var pfpCircle = GetPfpCircle();
                var circle = GetCircle();
                var achievIcons = await GetAchievementIcons(user, db);
                var avi = await GetAvatar(user, client);

                img.Mutate(x => x
                    .DrawImage(background, 1)
                    .DrawImage(templateBg, new Point(0, 0), gpOptions)
                    .DrawImage(avi, new Point(149, 8), aviOptions)
                    .DrawImage(pfpCircle, new Point(149, 8), gpOptions));
                img.Mutate(x =>
                    x.ApplyTextAsync(user.Username.Truncate(25), user.Id, user.Guild.Id, userdata, _levelGenerator).GetAwaiter()
                        .GetResult());
                img.Mutate(x => x.ApplyAchievementCircles(circle, achievIcons));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private static async Task<Image<Rgba32>> GetBackground(DbService db, HttpClient client,
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

        private static async Task<Stream> GetDefaultBackground(HttpClient client, DbService db)
        {
            var img = await db.Backgrounds.OrderBy(r => new Random().Next()).Take(1).FirstAsync();
            return await client.GetStreamAsync(img.BackgroundUrl);
        }

        private static Image<Rgba32> GetTemplateBackground()
        {
            using (var stream = new MemoryStream())
            {
                new FileStream("Data/Profile/Template/ProfileTemplate.png", FileMode.Open).CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var img = Image.Load(stream))
                {
                    return img.Clone();
                }
            }
        }

        private static Image<Rgba32> GetPfpCircle()
        {
            using (var img = Image.Load("Data/Profile/Template/ProfileCircle.png"))
            {
                return img.Clone();
            }
        }

        private static Image<Rgba32> GetCircle()
        {
            using (var img = Image.Load("Data/Profile/Template/ProfileCircle.png"))
            {
                img.Mutate(x => x.Resize(80, 80));
                return img.Clone();
            }
        }

        private static async Task<Image<Rgba32>> GetAvatar(IUser user, HttpClient client)
        {
            using (var avi = await client.GetStreamAsync(user.GetAvatar()))
            using (var img = Image.Load(avi))
            {
                return img.CloneAndConvertToAvatarWithoutApply(new Size(110, 110), 61).Clone();
            }
        }

        private static async Task<IEnumerable<Image<Rgba32>>> GetAchievementIcons(IGuildUser user, DbService db)
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

        private static async Task<List<AchievementMeta>> UnlockToAchieve(DbService db,
            IEnumerable<AchievementUnlock> list)
        {
            var result = new List<AchievementMeta>();
            foreach (var x in list) result.Add(await db.Achievements.FindAsync(x.AchievementId));

            return result;
        }

        private static async Task<Image<Rgba32>> GetIcon(AchievementMeta achiev)
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

        private static async Task<Image<Rgba32>> OnlineIcon(AchievementMeta achiev)
        {
            var client = new HttpClient();
            using (var image = Image.Load(await client.GetStreamAsync(achiev.ImageUrl)))
            {
                DownloadIcon(image, achiev);
                return image.CloneAndConvertToAvatarWithoutApply(new Size(80, 80), 50).Clone();
            }
        }

        private static void DownloadIcon(Image<Rgba32> image, AchievementMeta achiev)
        {
            image.Save($"Data/Achievement/Image/{achiev.AchievementNameId}/{achiev.AchievementId}.png");
        }

        private static async Task<Image<Rgba32>> GetHighestStackable(AchievementMeta achiev,
            IEnumerable<AchievementMeta> list, DbService db)
        {
            var achievementUnlocks = list.ToList();
            var max = achievementUnlocks.Where(x => x.AchievementNameId == achiev.AchievementNameId)
                .Max(x => x.Requirement);
            var result = achievementUnlocks.First(x =>
                x.AchievementNameId == achiev.AchievementNameId && x.Requirement == max);
            return await GetIcon(result);
        }
    }
}