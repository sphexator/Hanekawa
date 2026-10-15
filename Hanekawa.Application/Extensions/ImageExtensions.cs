using ImageMagick;
using ImageMagick.Drawing;

namespace Hanekawa.Application.Extensions;

public static class ImageExtensions
{
    public static void ConvertToAvatar(this IMagickImage image, int size, int cornerRadius)
    {
        image.Resize(new MagickGeometry((uint)size, (uint)size) { FillArea = true });
        image.Crop((uint)size, (uint)size, Gravity.Center);
        image.ApplyRoundedCorners(cornerRadius);
    }

    private static void ApplyRoundedCorners(this IMagickImage image, int cornerRadius)
    {
        using var mask = new MagickImage(MagickColors.Black, image.Width, image.Height);
        new Drawables()
            .StrokeColor(MagickColors.White)
            .FillColor(MagickColors.White)
            .RoundRectangle(0, 0, image.Width - 1, image.Height - 1, cornerRadius, cornerRadius)
            .Draw(mask);
        mask.Alpha(AlphaAction.Off);
        image.Alpha(AlphaAction.On);
        image.Composite(mask, CompositeOperator.CopyAlpha);
    }
}