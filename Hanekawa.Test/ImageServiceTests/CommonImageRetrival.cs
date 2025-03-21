using SixLabors.Fonts;

namespace Hanekawa.Test.ImageServiceTests;

public static class CommonImageRetrival
{
    public static FontCollection GetTestFontCollection()
    {
        var fontCollection = new FontCollection();
        fontCollection.Add(@"Data/Fonts/ARIAL.TTF");
        fontCollection.Add(@"Data/Fonts/TIMES.TTF");
        fontCollection.AddSystemFonts();
        return fontCollection;
    }
}