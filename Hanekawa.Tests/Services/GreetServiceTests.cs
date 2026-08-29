using Hanekawa.Application.Handlers.Commands.Settings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using OneOf.Types;

namespace Hanekawa.Tests.Services;

public class GreetServiceTests
{
    private const ulong GuildId = 1;

    [Fact]
    public async Task SetChannel_CreatesConfig_AndPersistsChannel()
    {
        GuildConfig? added = null;
        var dbSet = new List<GuildConfig>().BuildMockDbSet();
        dbSet.Setup(x => x.AddAsync(It.IsAny<GuildConfig>(), It.IsAny<CancellationToken>()))
            .Callback<GuildConfig, CancellationToken>((config, _) => added = config)
            .Returns(ValueTask.FromResult<EntityEntry<GuildConfig>>(null!));
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).Returns(dbSet.Object);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);
        var channel = new TextChannel { Id = 5, Name = "greet", GuildId = GuildId, Mention = "<#5>" };

        var result = await sut.SetChannel(GuildId, channel);

        Assert.Contains("<#5>", result);
        Assert.NotNull(added);
        Assert.Equal(5ul, added.GreetConfig!.Channel);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetMessage_UpdatesExistingGreetConfig()
    {
        var greet = new GreetConfig { GuildId = GuildId, Message = "old" };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, GreetConfig = greet } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var result = await sut.SetMessage(GuildId, "welcome {user}");

        Assert.Equal("Updated greet message !", result);
        Assert.Equal("welcome {user}", greet.Message);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetImage_AppendsImageToExistingConfig()
    {
        var greet = new GreetConfig { GuildId = GuildId, Images = [] };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, GreetConfig = greet } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var result = await sut.SetImage(GuildId, "https://img/welcome.png", 42);

        Assert.Equal("Updated greet image !", result);
        var image = Assert.Single(greet.Images);
        Assert.Equal("https://img/welcome.png", image.ImageUrl);
        Assert.Equal(42ul, image.Uploader);
        Assert.Equal(GuildId, image.GuildId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListImages_ReturnsNotFound_WhenNoImagesExist()
    {
        var configs = new List<GuildConfig>
        {
            new() { GuildId = GuildId, GreetConfig = new GreetConfig { GuildId = GuildId, Images = [] } }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var result = await sut.ListImages(GuildId);

        Assert.True(result.IsT0);
        Assert.IsType<NotFound>(result.AsT0);
    }

    [Fact]
    public async Task ListImages_ReturnsImages_WhenPresent()
    {
        var image = new GreetImage { Id = 3, GuildId = GuildId, ImageUrl = "https://img" };
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                GreetConfig = new GreetConfig { GuildId = GuildId, Images = [image] }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var result = await sut.ListImages(GuildId);

        Assert.True(result.IsT1);
        Assert.Same(image, Assert.Single(result.AsT1));
    }

    [Fact]
    public async Task RemoveImage_ReturnsFalse_WhenImageIsMissing()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                GreetConfig = new GreetConfig
                {
                    GuildId = GuildId,
                    Images = [new GreetImage { Id = 1, GuildId = GuildId, ImageUrl = "https://img" }]
                }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var removed = await sut.RemoveImage(GuildId, 99);

        Assert.False(removed);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveImage_RemovesMatchingImage()
    {
        var images = new List<GreetImage>
        {
            new() { Id = 1, GuildId = GuildId, ImageUrl = "https://keep" },
            new() { Id = 2, GuildId = GuildId, ImageUrl = "https://drop" }
        };
        var configs = new List<GuildConfig>
        {
            new() { GuildId = GuildId, GreetConfig = new GreetConfig { GuildId = GuildId, Images = images } }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var removed = await sut.RemoveImage(GuildId, 2);

        Assert.True(removed);
        Assert.Single(images);
        Assert.Equal(1, images[0].Id);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleImage_FlipsImageEnabled()
    {
        var greet = new GreetConfig { GuildId = GuildId, ImageEnabled = false };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, GreetConfig = greet } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new GreetService(db.Object, NullLogger<GreetService>.Instance);

        var enabled = await sut.ToggleImage(GuildId);
        var disabled = await sut.ToggleImage(GuildId);

        Assert.Equal("Enabled greet image !", enabled);
        Assert.Equal("Disabled greet image !", disabled);
        Assert.False(greet.ImageEnabled);
    }
}
