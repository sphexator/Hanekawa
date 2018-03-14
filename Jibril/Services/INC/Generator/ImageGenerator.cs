using System;
using System.Collections.Generic;
using Jibril.Services.INC.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.Primitives;

namespace Jibril.Services.INC.Generator
{
    public class ImageGenerator
    {
        public static IEnumerable<string> GenerateEventImage(List<Profile> profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new List<string>();
            var width = 0;
            var height = 0;
            var seat = 0;
            var row = 0;
            using (var img = new Image<Rgba32>(550, 550))
            {
                foreach (var x in profile)
                {
                    var points = GetBorderPointers(seat, row);
                    var hpBar = GetHeathBar(seat, row, x.Player.Damage);
                    var aviPath = $"Services/INC/Cache/Avatar/{x.Player.UserId}.png";
                    var avi = Image.Load(aviPath);
                    if (x.Player.Status == false)
                    {
                        var death = Image.Load("Services/INC/Cache/Avatar/DeathIndicator.png");
                        avi.Mutate(y => y
                            .BlackWhite()
                            .Resize(80, 80)
                            .DrawImage(death, new Size(80, 80), new Point(0, 0), GraphicsOptions.Default));
                    }

                    img.Mutate(a => a
                        .DrawImage(avi, new Size(80, 80), new Point(20 + 108 * width, 6 + 111 * height),
                            GraphicsOptions.Default)
                        .FillPolygon(new SolidBrush<Rgba32>(new Rgba32(30, 30, 30)), points)
                        .FillPolygon(new SolidBrush<Rgba32>(new Rgba32(0, 255, 0)), hpBar));
                    width++;
                    row++;
                    if (row != 5) continue;
                    height++;
                    row = 0;
                    width = 0;
                    seat++;
                }

                img.Mutate(x => x.Resize(300, 300));
                img.Save("Services/INC/Cache/Avatar/Banner.png");
                result.Add("Services/INC/Cache/Avatar/Banner.png");
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
            const int w1 = 10 - 1;
            const int w2 = 110 - 1;
            const int h1 = 86 - 1;
            const int h2 = 101 - 1;

            var point1 = new PointF(w1 + seat * 108, h1 + row * 111);
            var point2 = new PointF(w2 + seat * 108 - damage, h1 + row * 111);

            var point3 = new PointF(w2 + seat * 108 - damage, h2 + row * 111);
            var point4 = new PointF(w1 + seat * 108, h2 + row * 111);

            var result = new List<PointF> {point1, point2, point3, point4}.ToArray();
            return result;
        }

        private static int GetImageHeight(IReadOnlyCollection<Profile> profile)
        {
            if (profile.Count <= 5) return 106;
            if (profile.Count <= 10) return 207;
            if (profile.Count <= 15) return 308;
            return profile.Count <= 20 ? 409 : 510;
        }
    }
}