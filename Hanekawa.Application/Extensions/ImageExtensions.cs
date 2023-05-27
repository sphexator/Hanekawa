using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Hanekawa.Application.Extensions;

public static class ImageExtensions
{
    public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext processingContext, Size size, float cornerRadius) =>
        processingContext.Resize(new ResizeOptions
        {
            Size = size,
            Mode = ResizeMode.Crop
        }).ApplyRoundedCorners(cornerRadius);

    private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
    {
        var size = ctx.GetCurrentSize();
        var corners = BuildCorners(size.Width, size.Height, cornerRadius);

        ctx.SetGraphicsOptions(new GraphicsOptions()
        {
            Antialias = true,
            AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
        });

        foreach (var c in corners)
        {
            ctx = ctx.Fill(Color.Red, c);
        }
        return ctx;
    }

    private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
    {
        var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);
        
        var cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
        var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

        var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }
}