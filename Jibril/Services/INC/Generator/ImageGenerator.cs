using Jibril.Services.INC.Data;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp.Drawing.Pens;
using SixLabors.Primitives;

namespace Jibril.Services.INC.Generator
{
    public class ImageGenerator
    {
        public static IEnumerable<string> GenerateEventImage(List<Profile> profile)
        {
            var result = new List<string>();
            var width = 1;
            var height = 1;
            using (var img = new Image<Rgba32>(550, 550))
            {
                foreach (var x in profile)
                {
                    var points = GetBorderPointers(width, height);
                    var aviPath = $"Services/INC/Cache/Avatar/{x.Player.UserId}.png";
                    var avi = Image.Load(aviPath);
                    img.Mutate(a => a
                    .DrawImage(avi, new Size(80, 80), new Point((6 + 6 * height),(20 + 20 * width)), GraphicsOptions.Default)
                    .DrawPolygon(new SolidBrush<Rgba32>(Rgba32.DarkGray), 1, points));
                    var action = Events.EventHandler.EventManager(x);
                    width++;
                    if (width == 5 * height) height++;
                }
                img.Save("Services/INC/Cache/Avatar/Banner.png");
            }

            return result;
        }

        private static int GetImageHeight(IReadOnlyCollection<Profile> profile)
        {
            if (profile.Count <= 5) return 106;
            if (profile.Count <= 10) return 207;
            if (profile.Count <= 15) return 308;
            return profile.Count <= 20 ? 409 : 510;
        }

        private static PointF[] GetBorderPointers(int width, int height)
        {
            const int w1 = 6;
            const int w2 = 110;
            const int h1 = 86;
            const int h2 = 101;

            var point1 = new PointF(w1 * width, h1 * height);
            var point2 = new PointF(w2 * width, h1 * height);
            var point3 = new PointF(w1 * width, h2 * height);
            var point4 = new PointF(w2 * width, h2 * height);
            var result = new List<PointF> {point1, point2, point3, point4}.ToArray();
            return result;
        }
    }
}
