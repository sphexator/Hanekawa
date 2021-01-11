using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> ProfileBuilder(CachedMember user, DbService db)
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(400, 400))
            {
                var userData = await db.GetOrCreateUserData(user);
                var globalData = await db.GetOrCreateGlobalUserData(user);
                var progressBar = CreateProfileProgressBar(userData);
                // TODO: Create a inventory for backgrounds
                var background = await GetProfileBackground(db);
                var avi = await GetAvatarAsync(user, new Size(110, 110), false);

                var serverRank = await GetRankAsync(userData, db);
                var globalRank = await GetRankAsync(globalData, db);
                var achievePoints = await GetAchievementPoints(user, db);
                var color = new Color(new Rgba32(globalData.UserColor));

                img.Mutate(x =>
                {
                    x.DrawImage(background, 1);
                    x.DrawImage(_profileTemplate, new Point(0, 0), _options);
                    x.DrawImage(avi, new Point(145, 4), _options);
                    x.Fill(Color.Gray, new EllipsePolygon(200, 59, 55).GenerateOutline(4));
                    if (progressBar.Count >= 2)
                        x.DrawLines(color, 4, progressBar.ToArray());
                    var username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.DisplayName.Truncate(25)));
                    x.DrawText(_centerText, username, _profileName, Color.White, new PointF(200, 120));

                    //Text
                    x.DrawText(_leftText, "Server", _profileText, Color.White, new PointF(72, 160));
                    x.DrawText(_leftText, "Global", _profileText, Color.White, new PointF(270, 160));

                    // Server
                    x.DrawText(_leftText, "Rank", _profileText, Color.White, new PointF(8, 256));
                    x.DrawText(_rightText, $"{serverRank}", _profileText, Color.White, new PointF(194, 256));

                    x.DrawText(_leftText, "Level", _profileText, Color.White, new PointF(8, 184));
                    x.DrawText(_rightText, $"{userData.Level}", _profileText, Color.White, new PointF(194, 184));

                    x.DrawText(_leftText, "Exp", _profileText, Color.White, new PointF(8, 208));
                    x.DrawText(_rightText, $"{userData.Exp}", _profileText, Color.White, new PointF(194, 208));

                    x.DrawText(_leftText, "Credit", _profileText, Color.White, new PointF(8, 232));
                    x.DrawText(_rightText, $"{userData.Credit}", _profileText, Color.White, new PointF(194, 232));

                    x.DrawText(_leftText, "Achievement Points", _profileText, Color.White, new PointF(22, 286));
                    x.DrawText(_rightText, $"{achievePoints}", _profileText, Color.White, new PointF(377, 286));

                    // Global
                    x.DrawText(_leftText, "Rank", _profileText, Color.White, new PointF(206, 256));
                    x.DrawText(_rightText, $"{globalRank}", _profileText, Color.White, new PointF(391, 256));

                    x.DrawText(_leftText, "Credit", _profileText, Color.White, new PointF(206, 232));
                    x.DrawText(_rightText, $"{globalData.Credit}", _profileText, Color.White, new PointF(391, 232));

                    x.DrawText(_leftText, "Exp", _profileText, Color.White, new PointF(206, 208));
                    x.DrawText(_rightText, $"{globalData.Exp}", _profileText, Color.White, new PointF(391, 208));

                    x.DrawText(_leftText, "Level", _profileText, Color.White, new PointF(206, 184));
                    x.DrawText(_rightText, $"{globalData.Level}", _profileText, Color.White, new PointF(391, 184));
                });

                await img.SaveAsync(stream, new PngEncoder());
            }

            return stream;
        }

        private async Task<Image> GetProfileBackground(DbService db)
        {
            var backgroundList = await db.Backgrounds.ToListAsync();
            if (backgroundList == null || backgroundList.Count == 0)
            {
                var files = Directory.GetFiles("Data/Profile/default/");
                var file = files[_random.Next(files.Length)];
                using var img = Image.Load(file);
                return img.Clone(x => x.Resize(400, 400));
            }
            else
            {
                var background = await _client.GetStreamAsync(backgroundList[_random.Next(backgroundList.Count)].BackgroundUrl);
                var response = background.ToEditable();
                response.Position = 0;
                using var img = await Image.LoadAsync(response, new PngDecoder());
                return img.Clone(x => x.Resize(400, 400));
            }
        }

        private List<PointF> CreateProfileProgressBar(Account userData)
        {
            var perc = userData.Exp / (float) _expService.ExpToNextLevel(userData);
            var numb = perc * 100 / 100 * 360 * 2;
            var points = new List<PointF>();
            const double radius = 55;

            for (var i = 0; i < numb; i++)
            {
                var radians = i * Math.PI / 360;

                var x = 200 + radius * Math.Cos(radians - Math.PI / 2);
                var y = 59 + radius * Math.Sin(radians - Math.PI / 2);
                points.Add(new PointF((float) x, (float) y));
            }

            return points;
        }

        private async Task<string> GetRankAsync(Account userData, DbService db)
        {
            var total = await db.Accounts.CountAsync(x => x.GuildId == userData.GuildId);
            var rank = await db.Accounts.CountAsync(x =>
                x.TotalExp >= userData.TotalExp && x.GuildId == userData.GuildId);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private async Task<string> GetRankAsync(AccountGlobal userData, DbService db)
        {
            var total = await db.AccountGlobals.CountAsync();
            var rank = await db.AccountGlobals.CountAsync(x => x.TotalExp >= userData.TotalExp);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private async Task<string> GetAchievementPoints(CachedMember user, DbService db)
        {
            var points = await db.AchievementUnlocks.CountAsync(x => x.UserId == user.Id.RawValue);
            return $"{points * 10}";
        }
    }
}