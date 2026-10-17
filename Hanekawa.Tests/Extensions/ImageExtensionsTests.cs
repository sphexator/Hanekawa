using Hanekawa.Application.Extensions;
using ImageMagick;
using ImageMagick.Drawing;

namespace Hanekawa.Tests.Extensions;

public class ImageExtensionsTests
{
    [Fact]
    public void ConvertToAvatar_HalfSizeRadius_CutsCornersAndKeepsCenterAndEdges()
    {
        // CommonImageService uses this radius, which is a circle inscribed in the avatar.
        const int size = 20;
        var cornerRadius = (int)Math.Ceiling(size * Math.PI / (2 * Math.PI));
        using var image = new MagickImage(MagickColors.Red, 40, 16);

        image.ConvertToAvatar(size, cornerRadius);

        Assert.Equal((uint)size, image.Width);
        Assert.Equal((uint)size, image.Height);
        AssertTransparent(image, 0, 0);
        AssertTransparent(image, size - 1, 0);
        AssertTransparent(image, 0, size - 1);
        AssertTransparent(image, size - 1, size - 1);
        AssertOpaqueRed(image, size / 2, size / 2);
        AssertOpaqueRed(image, size / 2, 0);
        AssertOpaqueRed(image, size / 2, size - 1);
        AssertOpaqueRed(image, 0, size / 2);
        AssertOpaqueRed(image, size - 1, size / 2);
    }

    [Fact]
    public void ConvertToAvatar_CenterCropsNonSquareSource_ThenRoundsCorners()
    {
        using var image = new MagickImage(MagickColors.Blue, 60, 20);
        new Drawables()
            .FillColor(MagickColors.Red)
            .Rectangle(20, 0, 39, 19)
            .FillColor(MagickColors.Lime)
            .Rectangle(40, 0, 59, 19)
            .Draw(image);

        image.ConvertToAvatar(20, 4);

        Assert.Equal(20u, image.Width);
        Assert.Equal(20u, image.Height);
        AssertTransparent(image, 0, 0);
        AssertTransparent(image, 19, 19);
        AssertOpaqueRed(image, 10, 10);
        AssertOpaqueRed(image, 0, 10);
        AssertOpaqueRed(image, 19, 10);
    }

    private static void AssertTransparent(MagickImage image, int x, int y)
        => Assert.Equal<ushort>(0, ColorAt(image, x, y).A);

    private static void AssertOpaqueRed(MagickImage image, int x, int y)
    {
        var color = ColorAt(image, x, y);
        Assert.Equal(Quantum.Max, color.A);
        Assert.Equal(Quantum.Max, color.R);
        Assert.Equal<ushort>(0, color.G);
        Assert.Equal<ushort>(0, color.B);
    }

    private static IMagickColor<ushort> ColorAt(MagickImage image, int x, int y)
    {
        using var pixels = image.GetPixels();
        return pixels.GetPixel(x, y).ToColor()!;
    }
}
