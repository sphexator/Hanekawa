using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Hanekawa.Tests.Services;

public class AdministrationCommandServiceTests
{
    private readonly Mock<IBot> _bot = new();
    private readonly AdministrationCommandService _sut;
    private readonly DiscordMember _user = new()
    {
        Id = 10,
        Username = "user",
        Guild = new Guild { GuildId = 1, Name = "guild" }
    };

    public AdministrationCommandServiceTests()
    {
        _bot.Setup(x => x.BanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.UnbanAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.KickAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.MuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.UnmuteAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.PruneMessagesAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong[]>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()))
            .Returns(Task.CompletedTask);
        _bot.Setup(x => x.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<ulong>()))
            .Returns(Task.CompletedTask);

        _sut = new AdministrationCommandService(_bot.Object, NullLogger<AdministrationCommandService>.Instance);
    }

    [Fact]
    public async Task BanUserAsync_BansThroughBot_AndReturnsLocalizedMessage()
    {
        var result = await _sut.BanUserAsync(_user, 42, "spam", days: 3);

        _bot.Verify(x => x.BanAsync(1, 10, 3, "spam"), Times.Once);
        Assert.Equal(string.Format(Localization.BannedGuildUser, _user.Mention, _user.Guild.Name), result.Value.Content);
    }

    [Fact]
    public async Task UnbanUserAsync_UnbansThroughBot()
    {
        var result = await _sut.UnbanUserAsync(_user.Guild, 10, 42, "appeal");

        _bot.Verify(x => x.UnbanAsync(1, 10, "appeal"), Times.Once);
        Assert.Equal(string.Format(Localization.UnbannedGuildUser, 10ul, _user.Guild.Name), result.Value.Content);
    }

    [Fact]
    public async Task KickUserAsync_KicksThroughBot()
    {
        var result = await _sut.KickUserAsync(_user, 42, "spam");

        _bot.Verify(x => x.KickAsync(1, 10, "spam"), Times.Once);
        Assert.Equal(string.Format(Localization.KickedGuildUser, _user.Username, _user.Guild.Name), result.Value.Content);
    }

    [Fact]
    public async Task MuteUserAsync_MutesWithDuration()
    {
        var duration = TimeSpan.FromHours(2);

        var result = await _sut.MuteUserAsync(_user, 42, "spam", duration);

        _bot.Verify(x => x.MuteAsync(1, 10, "spam", duration), Times.Once);
        Assert.Contains(_user.Mention, result.Value.Content);
        Assert.StartsWith("Muted", result.Value.Content);
    }

    [Fact]
    public async Task UnmuteUserAsync_UnmutesThroughBot()
    {
        var result = await _sut.UnmuteUserAsync(_user, 42, "expired");

        _bot.Verify(x => x.UnmuteAsync(1, 10, "expired"), Times.Once);
        Assert.Equal(string.Format(Localization.UnMutedUser, _user.Mention), result.Value.Content);
    }

    [Fact]
    public async Task PruneAsync_PrunesMessageIds()
    {
        ulong[] messageIds = [11, 12, 13];

        var result = await _sut.PruneAsync(1, 99, messageIds, 42, "cleanup");

        _bot.Verify(x => x.PruneMessagesAsync(1, 99, messageIds), Times.Once);
        Assert.Equal(string.Format(Localization.PrunedMessages, 3), result.Value.Content);
    }

    [Fact]
    public async Task AddRoleAsync_AddsRoleThroughBot()
    {
        var result = await _sut.AddRoleAsync(_user, 42, 7);

        _bot.Verify(x => x.AddRoleAsync(1, 10, 7), Times.Once);
        Assert.Equal("", result.Value.Content);
    }

    [Fact]
    public async Task RemoveRoleAsync_RemovesRoleThroughBot()
    {
        var result = await _sut.RemoveRoleAsync(_user, 42, 7);

        _bot.Verify(x => x.RemoveRoleAsync(1, 10, 7), Times.Once);
        Assert.Equal("", result.Value.Content);
    }
}
