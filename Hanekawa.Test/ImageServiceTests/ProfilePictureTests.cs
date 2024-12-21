using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.Fonts;

namespace Hanekawa.Test.ImageServiceTests;

public class ProfilePictureTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<ImageService>> _loggerMock = new();
    private readonly ImageService _imageServiceMock;

    public ProfilePictureTests()
    {
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());
    }

    [Fact]
    public async Task GetProfilePictureAsync_WithValidUser_ReturnsProfilePicture()
    {
        // Arrange

        // Act
        Stream result = await _imageServiceMock.DrawProfileAsync(new DiscordMember(), new GuildUser(), CancellationToken.None);

        // Assert
    }

    [Fact]
    public async Task GetProfilePictureAsync_WithInvalidUser_ReturnsNull()
    {

    }
}