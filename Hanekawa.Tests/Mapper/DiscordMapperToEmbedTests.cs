using Disqord;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities.Discord;
using Moq;

namespace Hanekawa.Tests.Mapper;

public class DiscordMapperToEmbedTests
{
    [Fact]
    public void ToEmbed_MapsAuthorThumbnailImageFooterAndScalars()
    {
        var timestamp = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var author = new Mock<IEmbedAuthor>();
        author.SetupGet(x => x.Name).Returns("Hanekawa");
        author.SetupGet(x => x.IconUrl).Returns("https://cdn/avatar.png");
        author.SetupGet(x => x.Url).Returns("https://example.com");

        var footer = new Mock<IEmbedFooter>();
        footer.SetupGet(x => x.Text).Returns("Updated");
        footer.SetupGet(x => x.IconUrl).Returns("https://cdn/footer.png");

        var thumbnail = new Mock<IEmbedThumbnail>();
        thumbnail.SetupGet(x => x.Url).Returns("https://cdn/thumb.png");

        var image = new Mock<IEmbedImage>();
        image.SetupGet(x => x.Url).Returns("https://cdn/banner.png");

        var embed = new Mock<IEmbed>();
        embed.SetupGet(x => x.Title).Returns("Module status");
        embed.SetupGet(x => x.Description).Returns("Level is on");
        embed.SetupGet(x => x.Color).Returns(new Color(0x00FF00));
        embed.SetupGet(x => x.Timestamp).Returns(timestamp);
        embed.SetupGet(x => x.Author).Returns(author.Object);
        embed.SetupGet(x => x.Footer).Returns(footer.Object);
        embed.SetupGet(x => x.Thumbnail).Returns(thumbnail.Object);
        embed.SetupGet(x => x.Image).Returns(image.Object);

        var mapped = embed.Object.ToEmbed();

        Assert.Equal("Module status", mapped.Title);
        Assert.Equal("Level is on", mapped.Content);
        Assert.Equal(0x00FF00, mapped.Color);
        Assert.Equal("https://cdn/thumb.png", mapped.Icon);
        Assert.Equal("https://cdn/banner.png", mapped.Attachment);
        Assert.Equal(timestamp, mapped.Timestamp);
        Assert.NotNull(mapped.Header);
        Assert.Equal("Hanekawa", mapped.Header.Name);
        Assert.Equal("https://cdn/avatar.png", mapped.Header.IconUrl);
        Assert.Equal("https://example.com", mapped.Header.Url);
        Assert.NotNull(mapped.Footer);
        Assert.Equal("Updated", mapped.Footer.Text);
        Assert.Equal("https://cdn/footer.png", mapped.Footer.IconUrl);
    }
}
