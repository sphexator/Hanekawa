using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Entities.Settings.Images;
using Hanekawa.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Hanekawa.Test.ImageServiceTests;

public class RankPictureTests
{
    [Fact]
    public async Task DrawRankAsync_ReturnsStream()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var fontCollection = CommonImageRetrival.GetTestFontCollection();
        var imageSettings = new Mock<IOptionsMonitor<ImageSettings>>();
        var logger = new Mock<ILogger<ImageService>>();
        var dbContext = new Mock<IDbContext>();
        var configService = new Mock<IConfigService>();

        var imageService = new ImageService(httpClientFactory.Object, fontCollection,
                                            imageSettings.Object, logger.Object, dbContext.Object, configService.Object);
        var discordMember = TestUsers.TestMember;
        var guildUser = TestUsers.TestUser;
        
        // Act  
        var stream = await imageService.DrawRankAsync(discordMember, guildUser, CancellationToken.None);

        // Assert
        Assert.NotNull(stream);
    }
}