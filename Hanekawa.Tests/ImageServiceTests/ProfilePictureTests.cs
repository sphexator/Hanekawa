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

public class ProfilePictureTests
{
    // 1x1 PNG used so profile rendering stays offline and deterministic.
    private static readonly byte[] OneByOnePng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public async Task DrawProfileAsync_ReturnsStream()
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
            Profile = new ProfileSettings
            {
                Width = 400,
                Height = 400,
                Font = fontName,
                Avatar = new AvatarSettings { Size = 110, X = 145, Y = 4 },
                Texts = []
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

        var stream = await imageService.DrawProfileAsync(member, TestUsers.TestUser, CancellationToken.None);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
        stream.Position = 0;
        using var image = new MagickImage(stream);
        Assert.Equal(400, (int)image.Width);
        Assert.Equal(400, (int)image.Height);
    }

    [Fact]
    public async Task DrawProfileAsync_ZeroNextLevelExperience_DoesNotThrow()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new StaticImageHandler(OneByOnePng)));

        var fontCollection = CommonImageRetrival.GetTestFontCollection();
        var imageSettings = new Mock<IOptionsMonitor<ImageSettings>>();
        imageSettings.Setup(x => x.CurrentValue).Returns(new ImageSettings
        {
            Profile = new ProfileSettings
            {
                Width = 400,
                Height = 400,
                Font = "Arial",
                Avatar = new AvatarSettings { Size = 110, X = 145, Y = 4 },
                Texts =
                [
                    new TextSettings
                    {
                        TextType = "Regular",
                        Text = "Name",
                        Headline = true,
                        SourceType = "DiscordMember",
                        SourceField = "DisplayName",
                        Position = [new ImagePosition { X = 10, Y = 120 }, new ImagePosition { X = 390, Y = 120 }],
                        Size = 20
                    },
                    new TextSettings
                    {
                        TextType = "Regular",
                        Text = "Level",
                        SourceType = "GuildUser",
                        SourceField = "Level",
                        Position = [new ImagePosition { X = 15, Y = 160 }, new ImagePosition { X = 375, Y = 160 }],
                        Size = 20
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
            Nickname = null,
            AvatarUrl = "http://example.test/avatar.png"
        };
        var user = new GuildUser
        {
            Id = member.Id,
            GuildId = member.GuildId,
            Level = 4,
            Experience = 500,
            CurrentLevelExperience = 0,
            NextLevelExperience = 0
        };

        var stream = await imageService.DrawProfileAsync(member, user, CancellationToken.None);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
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