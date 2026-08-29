using System.Net;
using System.Net.Http.Headers;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Tests.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;

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
        var fontName = fontCollection.Families.First().Name;
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
        using var image = await Image.LoadAsync(stream);
        Assert.Equal(200, image.Width);
        Assert.Equal(80, image.Height);
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
