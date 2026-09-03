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

public class StreamServiceTests
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
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);
        var channel = new TextChannel { Id = 5, Name = "live", GuildId = GuildId, Mention = "<#5>" };

        var result = await sut.SetChannel(GuildId, channel);

        Assert.Contains("<#5>", result);
        Assert.NotNull(added);
        Assert.Equal(5ul, added.StreamConfig!.Channel);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TogglePublish_FlipsPublishOnStart()
    {
        var stream = new StreamConfig { GuildId = GuildId, PublishOnStart = false };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, StreamConfig = stream } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var enabled = await sut.TogglePublish(GuildId);
        var disabled = await sut.TogglePublish(GuildId);

        Assert.Equal("Enabled publishing when a configured user starts streaming !", enabled);
        Assert.Equal("Disabled publishing when a configured user starts streaming !", disabled);
        Assert.False(stream.PublishOnStart);
    }

    [Fact]
    public async Task AddUser_PersistsDiscordId_AndNormalizedTwitchLogin()
    {
        var stream = new StreamConfig { GuildId = GuildId, Users = [] };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, StreamConfig = stream } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.AddUser(GuildId, 42, "https://www.twitch.tv/CoolStreamer");

        Assert.Equal("Added Twitch coolstreamer for <@42> !", result);
        var user = Assert.Single(stream.Users);
        Assert.Equal(42ul, user.DiscordUserId);
        Assert.Equal("coolstreamer", user.TwitchLogin);
        Assert.Equal(GuildId, user.GuildId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddUser_RejectsDuplicateDiscordUser()
    {
        var stream = new StreamConfig
        {
            GuildId = GuildId,
            Users = [new StreamUser { GuildId = GuildId, DiscordUserId = 42, TwitchLogin = "alice" }]
        };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, StreamConfig = stream } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.AddUser(GuildId, 42, "bob");

        Assert.Equal("That Discord user is already configured for streaming.", result);
        Assert.Single(stream.Users);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddUser_RejectsDuplicateTwitchLogin()
    {
        var stream = new StreamConfig
        {
            GuildId = GuildId,
            Users = [new StreamUser { GuildId = GuildId, DiscordUserId = 1, TwitchLogin = "alice" }]
        };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, StreamConfig = stream } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.AddUser(GuildId, 99, "@Alice");

        Assert.Equal("That Twitch login is already configured for streaming.", result);
        Assert.Single(stream.Users);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddUser_RejectsInvalidTwitchLogin()
    {
        var stream = new StreamConfig { GuildId = GuildId, Users = [] };
        var configs = new List<GuildConfig> { new() { GuildId = GuildId, StreamConfig = stream } };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.AddUser(GuildId, 42, "not a login!");

        Assert.Equal("Twitch login is invalid.", result);
        Assert.Empty(stream.Users);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_ReturnsFalse_WhenUserIsMissing()
    {
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                StreamConfig = new StreamConfig
                {
                    GuildId = GuildId,
                    Users = [new StreamUser { GuildId = GuildId, DiscordUserId = 1, TwitchLogin = "alice" }]
                }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var removed = await sut.RemoveUser(GuildId, 99);

        Assert.False(removed);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUser_RemovesMatchingUser()
    {
        var users = new List<StreamUser>
        {
            new() { GuildId = GuildId, DiscordUserId = 1, TwitchLogin = "keep" },
            new() { GuildId = GuildId, DiscordUserId = 2, TwitchLogin = "drop" }
        };
        var configs = new List<GuildConfig>
        {
            new() { GuildId = GuildId, StreamConfig = new StreamConfig { GuildId = GuildId, Users = users } }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var removed = await sut.RemoveUser(GuildId, 2);

        Assert.True(removed);
        Assert.Single(users);
        Assert.Equal(1ul, users[0].DiscordUserId);
        db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListUsers_ReturnsNotFound_WhenNoUsersExist()
    {
        var configs = new List<GuildConfig>
        {
            new() { GuildId = GuildId, StreamConfig = new StreamConfig { GuildId = GuildId, Users = [] } }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.ListUsers(GuildId);

        Assert.True(result.IsT0);
        Assert.IsType<NotFound>(result.AsT0);
    }

    [Fact]
    public async Task ListUsers_ReturnsUsers_WhenPresent()
    {
        var user = new StreamUser { GuildId = GuildId, DiscordUserId = 7, TwitchLogin = "streamer" };
        var configs = new List<GuildConfig>
        {
            new()
            {
                GuildId = GuildId,
                StreamConfig = new StreamConfig { GuildId = GuildId, Users = [user] }
            }
        };
        var db = new Mock<IDbContext>();
        db.Setup(x => x.GuildConfigs).ReturnsDbSet(configs);
        var sut = new StreamService(db.Object, NullLogger<StreamService>.Instance);

        var result = await sut.ListUsers(GuildId);

        Assert.True(result.IsT1);
        Assert.Same(user, Assert.Single(result.AsT1));
    }

    [Theory]
    [InlineData("CoolStreamer", "coolstreamer")]
    [InlineData("@Alice", "alice")]
    [InlineData("https://twitch.tv/Bob", "bob")]
    [InlineData("https://www.twitch.tv/Foo_Bar", "foo_bar")]
    [InlineData("http://twitch.tv/name?foo=1", "name")]
    public void NormalizeTwitchLogin_StripsUrlAndCasing(string input, string expected)
        => Assert.Equal(expected, StreamService.NormalizeTwitchLogin(input));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a login!")]
    [InlineData("thisloginiswaytoolongtobevalid")]
    public void NormalizeTwitchLogin_ReturnsNull_WhenInvalid(string input)
        => Assert.Null(StreamService.NormalizeTwitchLogin(input));
}
