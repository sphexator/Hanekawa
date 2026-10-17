using Hanekawa.Application.Services.Images;

namespace Hanekawa.Tests.ImageServiceTests;

public class FontCollectionTests
{
    [Fact]
    public void Get_ResolvesByFileName_IgnoringCase()
    {
        var fonts = new FontCollection();
        fonts.Add("Data/Fonts/ARIAL.TTF");

        Assert.Equal("Data/Fonts/ARIAL.TTF", fonts.Get("arial"));
        Assert.Equal("Data/Fonts/ARIAL.TTF", fonts.Get("ARIAL"));
    }

    [Fact]
    public void Get_UnknownName_FallsBackToFirstRegisteredFont()
    {
        var fonts = new FontCollection();
        fonts.Add("Data/Fonts/TIMES.TTF");
        fonts.Add("Data/Fonts/ARIAL.TTF");

        Assert.Equal("Data/Fonts/TIMES.TTF", fonts.Get("does-not-exist"));
    }

    [Fact]
    public void Add_SameFontName_ReplacesPreviousPath()
    {
        var fonts = new FontCollection();
        fonts.Add("Data/Fonts/ARIAL.TTF");
        fonts.Add("Override/ARIAL.TTF");

        Assert.Equal("Override/ARIAL.TTF", fonts.Get("ARIAL"));
    }

    [Fact]
    public void Get_ExistingFilePath_ReturnsThatPath()
    {
        var path = Path.GetTempFileName();
        try
        {
            var fonts = new FontCollection();
            fonts.Add("Data/Fonts/ARIAL.TTF");

            Assert.Equal(path, fonts.Get(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
