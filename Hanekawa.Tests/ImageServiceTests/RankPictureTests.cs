using System.Net;
using System.Net.Http.Headers;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Entities.Users;
using Hanekawa.Tests.Common;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Hanekawa.Tests.ImageServiceTests;

public class RankPictureTests
{
    // 1x1 PNG used so rank rendering stays offline and deterministic.
    private static readonly byte[] OneByOnePng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public async Task DrawRankAsync_ReturnsStream()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new StaticImageHandler(OneByOnePng)));

        var fontCollection = CommonImageRetrival.GetTestFontCollection();
        var fontName = fontCollection.Names.First();
        var imageSettings = new Mock<IOptionsMonitor<ImageSettings>>();
        imageSettings.Setup(x => x.CurrentValue).Returns(new ImageSettings
        {
            Rank = new RankSettings
            {
                Width = 200,
                Height = 80,
                Font = fontName,
                Avatar = new AvatarSettings { Size = 32, X = 4, Y = 4 },
                Texts =
                [
                    new TextSettings
                    {
                        TextType = "Regular",
                        Text = "Level",
                        Size = 12,
                        Headline = true,
                        TextPosition = new ImagePosition { X = 50, Y = 20 }
                    }
                ]
            }
        });

        var imageService = new ImageService(
            httpClientFactory.Object,
            fontCollection,
            imageSettings.Object,
            new Mock<ILogger<ImageService>>().Object,
            new Mock<IDbContext>().Object,
            new Mock<IConfigService>().Object);

        var member = new DiscordMember
        {
            Id = TestUsers.TestMember.Id,
            GuildId = TestUsers.TestMember.GuildId,
            Guild = TestUsers.TestMember.Guild,
            Username = TestUsers.TestMember.Username,
            AvatarUrl = "http://example.test/avatar.png"
        };

        var stream = await imageService.DrawRankAsync(member, TestUsers.TestUser, CancellationToken.None);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
        stream.Position = 0;
        using var image = new MagickImage(stream);
        Assert.Equal(200, (int)image.Width);
        Assert.Equal(80, (int)image.Height);
    }

    [Fact]
    public async Task DrawRankAsync_CustomLevel_MatchesLiteralLevel_AndNotExperience()
    {
        var member = Member();
        var user = User(level: 12, experience: 340);

        var rendered = await RenderAsync(member, user, Custom("GuildUser", "Level"));
        var asLevel = await RenderAsync(member, user, Literal("12"));
        var asExperience = await RenderAsync(member, user, Literal("340"));

        Assert.Equal(asLevel, rendered);
        Assert.NotEqual(asExperience, rendered);
    }

    [Fact]
    public async Task DrawRankAsync_CustomExperience_MatchesLiteralExperience()
    {
        var member = Member();
        var user = User(level: 12, experience: 340);

        var rendered = await RenderAsync(member, user, Custom("GuildUser", "Experience"));
        var literal = await RenderAsync(member, user, Literal("340"));

        Assert.Equal(literal, rendered);
    }

    [Fact]
    public async Task DrawRankAsync_CustomUsername_MatchesLiteralUsername()
    {
        var member = Member();
        var user = User(level: 12, experience: 340);

        var rendered = await RenderAsync(member, user, Custom("GuildUser", "Username"));
        var literal = await RenderAsync(member, user, Literal(member.Username));

        Assert.Equal(literal, rendered);
    }

    private static DiscordMember Member() => new()
    {
        Id = TestUsers.TestMember.Id,
        GuildId = TestUsers.TestMember.GuildId,
        Guild = TestUsers.TestMember.Guild,
        Username = "Ada",
        AvatarUrl = "http://example.test/avatar.png"
    };

    private static GuildUser User(int level, long experience) => new()
    {
        Id = 1,
        GuildId = 1,
        Level = level,
        Experience = experience,
        User = new Hanekawa.Entities.Users.User { Id = 1 }
    };

    private static TextSettings Literal(string text) => new()
    {
        TextType = "Regular",
        Text = text,
        Size = 18,
        Headline = false,
        TextPosition = new ImagePosition { X = 48, Y = 16 }
    };

    private static TextSettings Custom(string sourceType, string sourceField) => new()
    {
        TextType = "Custom",
        SourceType = sourceType,
        SourceField = sourceField,
        Size = 18,
        Headline = false,
        TextPosition = new ImagePosition { X = 48, Y = 16 }
    };

    private static async Task<string> RenderAsync(DiscordMember member, GuildUser user, TextSettings text)
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new StaticImageHandler(OneByOnePng)));

        var fontCollection = CommonImageRetrival.GetTestFontCollection();
        var imageSettings = new Mock<IOptionsMonitor<ImageSettings>>();
        imageSettings.Setup(x => x.CurrentValue).Returns(new ImageSettings
        {
            Rank = new RankSettings
            {
                Width = 220,
                Height = 80,
                Font = fontCollection.Names.First(),
                Avatar = new AvatarSettings { Size = 32, X = 4, Y = 4 },
                Texts = [text]
            }
        });

        var imageService = new ImageService(
            httpClientFactory.Object,
            fontCollection,
            imageSettings.Object,
            new Mock<ILogger<ImageService>>().Object,
            new Mock<IDbContext>().Object,
            new Mock<IConfigService>().Object);

        var stream = await imageService.DrawRankAsync(member, user, CancellationToken.None);
        stream.Position = 0;
        using var image = new MagickImage(stream);
        return image.Signature;
    }

    private sealed class StaticImageHandler(byte[] bytes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return Task.FromResult(response);
        }
    }
}
