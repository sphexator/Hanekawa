using Disqord;
using Hanekawa.Bot.Mapper;
using Moq;

namespace Hanekawa.Tests.Mapper;

public class DiscordMapperToEmbedTests
{
    /// <summary>
    /// Covers <see cref="DiscordExtensions.ToEmbed"/> used when persisting REST send results in <c>Bot.SendMessageAsync</c>.
    /// </summary>
    [Fact]
    public void ToEmbed_MapsAuthorThumbnailImageFooterAndScalars()
    {
        var timestamp = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var author = new Mock<IEmbedAuthor>();
        author.Setup(x => x.Name).Returns("Hanekawa");
        author.Setup(x => x.IconUrl).Returns("https://cdn/avatar.png");
        author.Setup(x => x.Url).Returns("https://example.com");

        var thumbnail = new Mock<IEmbedThumbnail>();
        thumbnail.Setup(x => x.Url).Returns("https://cdn/thumb.png");

        var image = new Mock<IEmbedImage>();
        image.Setup(x => x.Url).Returns("https://cdn/banner.png");

        var footer = new Mock<IEmbedFooter>();
        footer.Setup(x => x.Text).Returns("Updated");
        footer.Setup(x => x.IconUrl).Returns("https://cdn/footer.png");

        var embed = new Mock<IEmbed>();
        embed.Setup(x => x.Title).Returns("Module status");
        embed.Setup(x => x.Description).Returns("Level is on");
        embed.Setup(x => x.Color).Returns(new Color(0x00FF00));
        embed.Setup(x => x.Timestamp).Returns(timestamp);
        embed.Setup(x => x.Author).Returns(author.Object);
        embed.Setup(x => x.Thumbnail).Returns(thumbnail.Object);
        embed.Setup(x => x.Image).Returns(image.Object);
        embed.Setup(x => x.Footer).Returns(footer.Object);

        var mapped = embed.Object.ToEmbed();

        Assert.Equal("Module status", mapped.Title);
        Assert.Equal("Level is on", mapped.Content);
        Assert.Equal("https://cdn/thumb.png", mapped.Icon);
        Assert.Equal("https://cdn/banner.png", mapped.Attachment);
        Assert.Equal(timestamp, mapped.Timestamp);
        Assert.Equal(0x00FF00, mapped.Color);
        Assert.NotNull(mapped.Header);
        Assert.Equal("Hanekawa", mapped.Header!.Name);
        Assert.Equal("https://cdn/avatar.png", mapped.Header.IconUrl);
        Assert.Equal("https://example.com", mapped.Header.Url);
        Assert.NotNull(mapped.Footer);
        Assert.Equal("Updated", mapped.Footer!.Text);
        Assert.Equal("https://cdn/footer.png", mapped.Footer.IconUrl);
    }
}
