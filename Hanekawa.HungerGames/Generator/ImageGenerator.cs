using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.HungerGames.Entities.Internal;
using Hanekawa.HungerGames.Entities.User;
using Hanekawa.HungerGames.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hanekawa.HungerGames.Generator
{
    internal class ImageGenerator : IRequired
    {
        private readonly HttpClient _httpClient;

        internal ImageGenerator(HttpClient httpClient) => _httpClient = httpClient;

        internal async Task<Stream> GenerateSingleImageAsync(HungerGameProfile profile)
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(550, 550))
            {

            }

            return stream;
        }

        internal async Task<Stream> GenerateEventImageAsync(IEnumerable<HungerGameProfile> profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new MemoryStream();
            using (var img = new Image<Rgba32>(550, 550))
            {
                var width = 0;
                var height = 0;
                var seat = 0;
                var row = 0;
                foreach (var x in profile)
                {
                    var points = GetBorderPointers(width, height);
                    var hpBar = GetHeathBar(width, height, x.Health);
                    var avi = await GetUserAvatar(x);
                    if (x.Health <= 0 && x.Alive) x.Alive = false;

                    // Profile picture drawing
                    if (!x.Alive)
                    {
                        var death = Image.Load(await _httpClient.GetStreamAsync("https://i.imgur.com/eONxWtN.png"));
                        death.Mutate(z => z.Resize(80, 80));
                        avi.Mutate(y => y
                            .BlackWhite()
                            .DrawImage(death, new Point(0, 0), new GraphicsOptions{ Antialias = true }));
                    }

                    // Healthbar drawing
                    img.Mutate(a => a
                        .DrawImage(avi, new Point(20 + 108 * width, 6 + 111 * height),
                            new GraphicsOptions {Antialias = true})
                        .FillPolygon(new SolidBrush(new Color(new Rgb24(30, 30, 30))), points));
                    if (x.Alive)
                        img.Mutate(a => a.FillPolygon(new SolidBrush(new Color(new Rgb24(46, 204, 113))), hpBar));

                    // Health text drawing
                    var healthTextLocation = GetHealthTextLocation(width, height);
                    var font = SystemFonts.CreateFont("Times New Roman", 15, FontStyle.Regular);
                    var hp = $"       {x.Health} / 100";
                    //img.Mutate(a => a.DrawText(hp, font, Rgba32.White, healthTextLocation));
                    img.Mutate(a => a.DrawText(hp, font, Color.White, healthTextLocation));
                    width++;
                    row++;
                    if (row != 5) continue;
                    height++;
                    row = 0;
                    width = 0;
                    seat++;
                }

                img.Mutate(x => x.Resize(400, 400));
                img.Save(result, new PngEncoder());
            }

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

            var result = new List<PointF> {point1, point2, point3, point4}.ToArray();
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
            var point2 = new PointF((float) (w2 + seat * 108 - health), h1 + row * 111);

            var point3 = new PointF((float) (w2 + seat * 108 - health), h2 + row * 111);
            var point4 = new PointF(w1 + seat * 108, h2 + row * 111);

            var result = new List<PointF> {point1, point2, point3, point4}.ToArray();
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

        private static int GetImageHeight(IReadOnlyCollection<HungerGameProfile> profile)
        {
            if (profile.Count <= 5) return 106;
            if (profile.Count <= 10) return 207;
            if (profile.Count <= 15) return 308;
            return profile.Count <= 20 ? 409 : 510;
        }

        private async Task<Image> GetUserAvatar(HungerGameProfile profile)
        {
            try
            {
                var avatar = await _httpClient.GetStreamAsync(profile.Avatar);
                var response = avatar.ToEditable();
                response.Position = 0;
                using var img = await Image.LoadAsync(response, new PngDecoder());
                return img.Clone(x => x.Resize(new Size(80, 80)));
            }
            catch
            {
                using var img = await Image.LoadAsync(@"Cache\DefaultAvatar\Default.png", new PngDecoder());
                return img.Clone(x => x.Resize(new Size(80, 80)));
            }
        }
    }
}