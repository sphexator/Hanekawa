using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Hanekawa.Bot.Service.ImageGeneration
{
    public partial class ImageGenerationService
    {
        public async Task<Stream> ProfileBuilder(IMember user, DbService db)
        {
            var stream = new MemoryStream();
            using var img = new Image<Rgba32>(400, 400);

            var userData = await db.GetOrCreateUserData(user);
            var globalData = await db.GetOrCreateGlobalUserDataAsync(user);
            var progressBar = CreateProfileProgressBar(userData);
            // TODO: Create a inventory for backgrounds
            var background = await GetProfileBackground(db);
            var avi = await GetAvatarAsync(user, new Size(110, 110), false);

            var serverRank = await GetRankAsync(userData, db);
            var globalRank = await GetRankAsync(globalData, db);
            var achievePoints = await GetAchievementPoints(user, db);
            var color = new Color(new Rgba32((uint)globalData.UserColor));
            
            var profileText = new Font(_arial, 20, FontStyle.Regular);
            var profileName = new Font(_arial, 32, FontStyle.Regular);
            
            img.Mutate(x =>
            {
                x.DrawImage(background, 1);
                x.DrawImage(_profileTemplate, new Point(0, 0), _options);
                x.DrawImage(avi, new Point(145, 4), _options);
                x.Fill(Color.Gray, new EllipsePolygon(200, 59, 55).GenerateOutline(4));
                if (progressBar.Count >= 2)
                    x.DrawLines(color, 4, progressBar.ToArray());
                try
                {
                    x.DrawText(_centerText, user.Nick.Truncate(25), profileName, Color.White, new PointF(200, 120));
                }
                catch
                {
                    var username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.Nick.Truncate(25)));
                    x.DrawText(_centerText, username, profileName, Color.White, new PointF(200, 120));
                }

                //Text
                x.DrawText(_leftText, "Server", profileText, Color.White, new PointF(72, 160));
                x.DrawText(_leftText, "Global", profileText, Color.White, new PointF(270, 160));

                // Server
                x.DrawText(_leftText, "Rank", profileText, Color.White, new PointF(8, 256));
                x.DrawText(_rightText, $"{serverRank}", profileText, Color.White, new PointF(194, 256));

                x.DrawText(_leftText, "Level", profileText, Color.White, new PointF(8, 184));
                x.DrawText(_rightText, $"{userData.Level}", profileText, Color.White, new PointF(194, 184));

                x.DrawText(_leftText, "Exp", profileText, Color.White, new PointF(8, 208));
                x.DrawText(_rightText, $"{userData.Exp}", profileText, Color.White, new PointF(194, 208));

                x.DrawText(_leftText, "Credit", profileText, Color.White, new PointF(8, 232));
                x.DrawText(_rightText, $"{userData.Credit}", profileText, Color.White, new PointF(194, 232));

                x.DrawText(_leftText, "Achievement Points", profileText, Color.White, new PointF(22, 286));
                x.DrawText(_rightText, $"{achievePoints}", profileText, Color.White, new PointF(377, 286));

                // Global
                x.DrawText(_leftText, "Rank", profileText, Color.White, new PointF(206, 256));
                x.DrawText(_rightText, $"{globalRank}", profileText, Color.White, new PointF(391, 256));

                x.DrawText(_leftText, "Credit", profileText, Color.White, new PointF(206, 232));
                x.DrawText(_rightText, $"{globalData.Credit}", profileText, Color.White, new PointF(391, 232));

                x.DrawText(_leftText, "Exp", profileText, Color.White, new PointF(206, 208));
                x.DrawText(_rightText, $"{globalData.Exp}", profileText, Color.White, new PointF(391, 208));

                x.DrawText(_leftText, "Level", profileText, Color.White, new PointF(206, 184));
                x.DrawText(_rightText, $"{globalData.Level}", profileText, Color.White, new PointF(391, 184));
            });

            await img.SaveAsync(stream, new PngEncoder());

            return stream;
        }

        private async Task<Image> GetProfileBackground(DbService db)
        {
            var backgroundList = await db.Backgrounds.ToListAsync();
            if (backgroundList == null || backgroundList.Count == 0)
            {
                var files = Directory.GetFiles("Data/Profile/default/");
                var file = files[_random.Next(files.Length)];
                using var img = await Image.LoadAsync(file);
                return img.Clone(x => x.Resize(400, 400));
            }
            else
            {
                var background =
                    await _http.CreateClient().GetStreamAsync(backgroundList[_random.Next(backgroundList.Count)].BackgroundUrl);
                var response = background.ToEditable();
                response.Position = 0;
                using var img = await Image.LoadAsync(response, new PngDecoder());
                return img.Clone(x => x.Resize(400, 400));
            }
        }

        private List<PointF> CreateProfileProgressBar(Account userData)
        {
            var percentage = userData.Exp / (float) _experience.ExpToNextLevel(userData);
            var numb = percentage * 100 / 100 * 360 * 2;
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

        private static async Task<string> GetRankAsync(Account userData, DbService db)
        {
            var total = await db.Accounts.CountAsync(x => x.GuildId == userData.GuildId);
            var rank = await db.Accounts.CountAsync(x =>
                x.TotalExp >= userData.TotalExp && x.GuildId == userData.GuildId && x.Active);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private static async Task<string> GetRankAsync(AccountGlobal userData, DbService db)
        {
            var total = await db.AccountGlobals.CountAsync();
            var rank = await db.AccountGlobals.CountAsync(x => x.TotalExp >= userData.TotalExp);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private static async Task<string> GetAchievementPoints(ISnowflakeEntity user, DbService db)
        {
            var points = await db.AchievementUnlocks.CountAsync(x => x.UserId == user.Id.RawValue);
            return $"{points * 10}";
        }
    }
}