using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
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
                var userdata = await _db.GetOrCreateUserData(user);
                var progBar = CreateProfileProgressBarAsync(userdata);
                // TODO: Create a inventory for backgrounds
                var background = GetProfileBackground();
                var avi = GetAvatarAsync(user, new Size(110, 110), 61);

                await Task.WhenAll(progBar, background, avi);

                img.Mutate(x =>
                {
                    x.DrawImage(background.Result, 1);
                    x.DrawImage(_welcomeTemplate, new Point(0, 0), _options);
                    x.DrawImage(avi.Result, new Point(145, 4), _options);
                    x.Fill(_options, Rgba32.Gray, new EllipsePolygon(200, 59, 55).GenerateOutline(4));
                    if (progBar.Result.Count >= 2) // TODO: Make the color scheme of the profile customizable
                        x.DrawLines(_options, Rgba32.BlueViolet, 4, progBar.Result.ToArray());

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

        private async Task<List<PointF>> CreateProfileProgressBarAsync(Account userdata)
        {
            var perc = userdata.Exp / (float) _expService.ExpToNextLevel(userdata);
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
    }
}