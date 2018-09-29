using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using Image = SixLabors.ImageSharp.Image;

namespace Hanekawa.Addons.HungerGame.Generator
{
    public class ImageGenerator
    {
        private DiscordSocketClient _client { get; }

        public async Task<Stream> GenerateEventImageAsync(IEnumerable<HungerGameLive> profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new MemoryStream();
            var width = 0;
            var height = 0;
            var seat = 0;
            var row = 0;
            using (var client = new HttpClient())
            using (var img = new Image<Rgba32>(550, 550))
            {
                foreach (var x in profile)
                {
                    var points = GetBorderPointers(width, height);
                    var hpBar = GetHeathBar(width, height, x.Health);
                    //var aviPath = $"Services/INC/Cache/Avatar/{x.UserId}.png";
                    var aviPath = await client.GetStreamAsync(GetUserAvatar(x.UserId));
                    var avi = Image.Load(aviPath);
                    if (x.Status == false)
                    {
                        var death = Image.Load((await client.GetStreamAsync("https://i.imgur.com/eONxWtN.png")));
                        avi.Mutate(y => y
                            .BlackWhite()
                            .Resize(80, 80)
                            .DrawImage(GraphicsOptions.Default, death, new Point(0, 0)));
                    }

                    img.Mutate(a => a
                        .DrawImage(GraphicsOptions.Default, avi, new Point(20 + 108 * width, 6 + 111 * height))
                        .FillPolygon(new SolidBrush<Rgba32>(new Rgba32(30, 30, 30)), points));
                    if (x.Status)
                    {
                        img.Mutate(a => a.FillPolygon(new SolidBrush<Rgba32>(new Rgba32(0, 255, 0)), hpBar));
                    }

                    var path = GetHealthTextLocation(width, height);
                    var font = SystemFonts.CreateFont("Times New Roman", 15, FontStyle.Regular);
                    var hp = $"       {x.Health} / 100";
                    var optionsCenter = new TextGraphicsOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    img.Mutate(a => a.DrawText(optionsCenter, hp, font, Rgba32.White, path));
                    img.Mutate(a => a.DrawText(hp, font, Rgba32.White, path));
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

        private static PointF[] GetHeathBar(int seat, int row, int damage)
        {
            //Size of box
            const int w1 = 10 + 3;
            const int w2 = 110 - 3;
            const int h1 = 86 + 3;
            const int h2 = 101 - 3;

            var point1 = new PointF(w1 + seat * 108, h1 + row * 111);
            var point2 = new PointF(w2 + seat * 108 - damage, h1 + row * 111);

            var point3 = new PointF(w2 + seat * 108 - damage, h2 + row * 111);
            var point4 = new PointF(w1 + seat * 108, h2 + row * 111);

            var result = new List<PointF> {point1, point2, point3, point4}.ToArray();
            return result;
        }

        private static PointF GetHealthTextLocation(int seat, int row)
        {
            const int starterWidth = 10;
            const int spacerWidth = 108;
            const int starterHeight = 99;
            const int spacerHeight = 111;
            
            return new PointF(starterWidth + (seat * spacerWidth), starterHeight + (row * spacerHeight));
        }

        private static int GetImageHeight(IReadOnlyCollection<Profile> profile)
        {
            if (profile.Count <= 5) return 106;
            if (profile.Count <= 10) return 207;
            if (profile.Count <= 15) return 308;
            return profile.Count <= 20 ? 409 : 510;
        }

        private string GetUserAvatar(ulong userid)
        {
            var user = _client.GetUser(userid);
            if (user != null) return user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl();
            else return null;
        }
    }
}