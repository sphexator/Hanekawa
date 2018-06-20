using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System.Numerics;

namespace Jibril.Extensions
{
    public static class ImageExtension
    {
        public static IImageProcessingContext<Rgba32> ConvertToAvatar(
            this IImageProcessingContext<Rgba32> processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).Apply(i => ApplyRoundedCorners(i, cornerRadius));
        }

        public static Image<Rgba32> CloneAndConvertToAvatarWithoutApply(this Image<Rgba32> image, Size size,
            float cornerRadius)
        {
            var result = image.Clone(
                ctx => ctx.Resize(
                    new ResizeOptions
                    {
                        Size = size,
                        Mode = ResizeMode.Crop
                    }));

            ApplyRoundedCorners(result, cornerRadius);
            return result;
        }

        private static void ApplyRoundedCorners(Image<Rgba32> img, float cornerRadius)
        {
            var corners = BuildCorners(img.Width, img.Height, cornerRadius);


            img.Mutate(x => x.Fill(Rgba32.Transparent, corners));
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularePolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);
            var cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var center = new Vector2(imageWidth / 2F, imageHeight / 2F);

            var rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            var cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}
