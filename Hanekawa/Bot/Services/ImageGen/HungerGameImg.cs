using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Extensions;
using Hanekawa.Models.HungerGame;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.Bot.Services.ImageGen
{
    public partial class ImageGenerator
    {
        public async Task<Stream> GenerateSingleImageAsync(HungerGameProfile profile)
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(550, 550))
            {

            }

            return stream;
        }

        public async Task<Stream> GenerateEventImageAsync(List<UserAction> profile, int alive)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new MemoryStream();
            using (var img = new Image<Rgba32>(550, GetImageHeight(alive)))
            {
                var width = 0;
                var height = 0;
                var seat = 0;
                var row = 0;
                for (var i = 0; i < profile.Count; i++)
                {
                    var x = profile[i];
                    var points = GetBorderPointers(width, height);
                    var hpBar = GetHeathBar(width, height, x.AfterProfile.Health);
                    var avi = await GetAvatarAsync(x.AfterProfile.Avatar, new Size(80, 80));
                    if (x.AfterProfile.Health <= 0 && x.AfterProfile.Alive) x.AfterProfile.Alive = false;

                    // Profile picture drawing
                    if (!x.AfterProfile.Alive)
                    {
                        var deathIcon = await _client.GetStreamAsync("https://i.imgur.com/eONxWtN.png");
                        var response = deathIcon.ToEditable();
                        response.Position = 0;
                        var death = await Image.LoadAsync(response, new PngDecoder());
                        death.Mutate(z => z.Resize(80, 80));
                        avi.Mutate(y => y
                            .BlackWhite()
                            .DrawImage(death, new Point(0, 0), new GraphicsOptions { Antialias = true }));
                    }

                    // Health bar drawing
                    img.Mutate(a => a
                        .DrawImage(avi, new Point(20 + 108 * width, 6 + 111 * height),
                            new GraphicsOptions {Antialias = true})
                        .FillPolygon(new SolidBrush(new Color(new Rgb24(30, 30, 30))), points));
                    if (x.AfterProfile.Alive)
                        img.Mutate(a => a.FillPolygon(new SolidBrush(new Color(new Rgb24(46, 204, 113))), hpBar));

                    // Health text drawing
                    var healthTextLocation = GetHealthTextLocation(width, height);
                    var hp = $"       {x.AfterProfile.Health} / 100";
                    img.Mutate(a => a.DrawText(hp, _hgTimes, Color.White, healthTextLocation));
                    
                    width++;
                    row++;
                    if (row != 5) continue;
                    height++;
                    row = 0;
                    width = 0;
                    seat++;
                }

                await img.SaveAsync(result, new PngEncoder());
            }

            result.Position = 0;
            return result;
        }

        private static PointF[] GetBorderPointers(int seat, int row)
        {
            //Size of box
            const int w1 = 10;
            const int w2 = 110;
            const int h1 = 86;
            const int h2 = 101;

            var point1 = new PointF(w1 + seat * 108, h1 + row * 111);
            var point2 = new PointF(w2 + seat * 108, h1 + row * 111);

            var point3 = new PointF(w2 + seat * 108, h2 + row * 111);
            var point4 = new PointF(w1 + seat * 108, h2 + row * 111);

            var result = new[] { point1, point2, point3, point4 };
            return result;
        }

        private static PointF[] GetHeathBar(int seat, int row, double health)
        {
            //Size of box
            const int w1 = 10 + 3;
            const int w2 = 110 - 3;
            const int h1 = 86 + 2;
            const int h2 = 101 - 2;

            health = 100 - health;
            if (health < 0) health = 1;
            if (health >= 100) health = 100;

            var point1 = new PointF(w1 + seat * 108, h1 + row * 111);
            var point2 = new PointF((float)(w2 + seat * 108 - health), h1 + row * 111);

            var point3 = new PointF((float)(w2 + seat * 108 - health), h2 + row * 111);
            var point4 = new PointF(w1 + seat * 108, h2 + row * 111);

            var result = new[] {point1, point2, point3, point4};
            return result;
        }

        private static PointF GetHealthTextLocation(int seat, int row)
        {
            const int starterWidth = 10;
            const int spacerWidth = 108;
            const int starterHeight = 88;
            const int spacerHeight = 111;

            return new PointF(starterWidth + seat * spacerWidth, starterHeight + row * spacerHeight);
        }

        private static int GetImageHeight(int amount) =>
            amount <= 5 ? 106 :
            amount <= 10 ? 207 :
            amount <= 15 ? 308 :
            amount <= 20 ? 409 : 510;
    }
}