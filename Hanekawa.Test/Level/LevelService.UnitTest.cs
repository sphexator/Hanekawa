using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hanekawa.Test.Level;

public class LevelServiceUnitTest
{
    private readonly LevelService _levelService;
    private readonly Mock<ILogger<LevelService>> _mockLogger = new();
    private readonly Mock<IDbContext> _mockdb = new();
    private readonly Mock<IBot> _mockBot = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private DiscordMember member = new() { Id = 1, Guild = new() { Id = 1 }, Username = "Bob", RoleIds = new() { 1, 2, 3 } };
    private GuildConfig _config = new ()
    {
        GuildId = 1,
        LevelConfig = new ()
        {
            GuildId = 1,
            Multiplier = 1,
            DecayEnabled = false,
            LevelEnabled = true,
            MultiplierEnd = DateTimeOffset.MinValue
        }
    };

    public LevelServiceUnitTest() 
        => _levelService = new (_mockdb.Object, _mockLogger.Object, _mockBot.Object, _mockMediator.Object);

    [Fact]
    public async Task LevelService_AddExperienceAsync_Returns_100()
    {
        // Arrange
        _mockdb.SetupGet(e => e.GuildConfigs
            .Include(x => x.LevelConfig)
            .ThenInclude(x => x.Rewards)
            .FirstOrDefault(x => x.GuildId == member.Guild.Id)
            ).Returns(_config);
        
        const int experience = 100;
        const int expected = 100;
        
        // Act
        var actual = await _levelService.AddExperienceAsync(member, experience);
        
        // Assert
        Assert.Equal(expected, actual);
    }
}