using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> ProfileBuilder(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(400, 400))
            {
                var userData = await _db.GetOrCreateUserData(user);
                var globalData = await _db.GetOrCreateGlobalUserData(user);
                var progressBar = CreateProfileProgressBar(userData);
                // TODO: Create a inventory for backgrounds
                var background = GetProfileBackground();
                var avi = GetAvatarAsync(user, new Size(110, 110), 61);

                var serverRank = await GetRankAsync(user, userData);
                var globalRank = await GetRankAsync(user, globalData);
                var achievePoints = await GetAchievementPoints(user);

                await Task.WhenAll(background, avi);

                img.Mutate(x =>
                {
                    x.DrawImage(background.Result, 1);
                    x.DrawImage(_profileTemplate, new Point(0, 0), _options);
                    x.DrawImage(avi.Result, new Point(145, 4), _options);
                    x.Fill(_options, Rgba32.Gray, new EllipsePolygon(200, 59, 55).GenerateOutline(4));
                    if (progressBar.Count >= 2) // TODO: Make the color scheme of the profile customizable
                        x.DrawLines(_options, Rgba32.BlueViolet, 4, progressBar.ToArray());
                    var username = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(user.Username.Truncate(25)));
                    x.DrawText(_centerText, username, _profileName, Rgba32.White, new PointF(200, 120));

                    //Text
                    x.DrawText(_centerText, "Server", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_centerText, "Global", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Rank", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{serverRank}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Level", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{userData.Level}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Exp", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{userData.Exp}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Credit", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{userData.Credit}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Special Credit", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{userData.CreditSpecial}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Achievement Points", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{achievePoints}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Global Rank", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{globalRank}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Global Credit", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{globalData.Credit}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Global Exp", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{globalData.Exp}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Global TotalExp", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{globalData.TotalExp}", _profileText, Rgba32.White, new PointF());

                    x.DrawText(_leftText, "Global Level", _profileText, Rgba32.White, new PointF());
                    x.DrawText(_rightText, $"{globalData.Level}", _profileText, Rgba32.White, new PointF());
                });

                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private async Task<Image<Rgba32>> GetProfileBackground()
        {
            var background = await _db.Backgrounds.OrderBy(r => _random.Next()).Take(1).FirstAsync();
            using (var img = Image.Load(await _client.GetStreamAsync(background.BackgroundUrl)))
            {
                img.Mutate(x => x.Resize(400, 400));
                return img.Clone();
            }
        }

        private List<PointF> CreateProfileProgressBar(Account userData)
        {
            var perc = userData.Exp / (float)_expService.ExpToNextLevel(userData);
            var numb = perc * 100 / 100 * 360 * 2;
            var points = new List<PointF>();
            const double radius = 55;

            for (var i = 0; i < numb; i++)
            {
                var radians = i * Math.PI / 360;

                var x = 200 + radius * Math.Cos(radians - Math.PI / 2);
                var y = 59 + radius * Math.Sin(radians - Math.PI / 2);
                points.Add(new PointF((float)x, (float)y));
            }

            return points;
        }

        private async Task<string> GetRankAsync(SocketGuildUser user, Account userdata)
        {
            var total = await _db.Accounts.CountAsync(x => x.GuildId == user.Guild.Id);
            var rank = await _db.Accounts.CountAsync(x =>
                x.TotalExp >= userdata.TotalExp && x.GuildId == user.Guild.Id);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private async Task<string> GetRankAsync(SocketGuildUser user, AccountGlobal userdata)
        {
            var total = await _db.AccountGlobals.CountAsync();
            var rank = await _db.AccountGlobals.CountAsync(x => x.TotalExp >= userdata.TotalExp);
            return $"{rank.FormatNumber()}/{total.FormatNumber()}";
        }

        private async Task<string> GetAchievementPoints(SocketGuildUser user)
        {
            var points = await _db.AchievementUnlocks.CountAsync(x => x.UserId == user.Id);
            return $"{points * 10}";
        }
    }
}