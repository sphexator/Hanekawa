using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Test.Level;

public class LevelServiceUnitTest
{
    private readonly LevelService _levelService;
    private readonly Mock<ILogger<LevelService>> _mockLogger = new();
    private readonly Mock<IDbContext> _mockdb = new();
    private readonly Mock<IBot> _mockBot = new();
    private readonly Mock<IMediator> _mockMediator = new();
    
    private readonly DiscordMember member = new()
        { Id = 1, Guild = new() { Id = 1 }, Username = "Bob", RoleIds = new() { 1, 2, 3 } };
    private readonly GuildConfig _config = new ()
    {
        GuildId = 1,
        LevelConfig = new ()
        {
            GuildId = 1,
            Multiplier = 1,
            DecayEnabled = false,
            LevelEnabled = true,
            MultiplierEnd = DateTimeOffset.MinValue,
            Rewards = new ()
            {
                new() { GuildId = 1, Level = 1, Money = 0, RoleId = 3 },
                new() { GuildId = 1, Level = 2, Money = 100, RoleId = 4 },
                new() { GuildId = 1, Level = 3, Money = 200, RoleId = 5 },
                new() { GuildId = 1, Level = 4, Money = 300, RoleId = 6 },
                new() { GuildId = 1, Level = 5, Money = 400, RoleId = 7 },
                new() { GuildId = 1, Level = 6, Money = 500, RoleId = 8 },
                new() { GuildId = 1, Level = 7, Money = 600, RoleId = 9 },
                new() { GuildId = 1, Level = 8, Money = 700, RoleId = 10 },
            }
        }
    };
    
    private readonly GuildUser _user = new()
    {
        GuildId = 1,
        UserId = 1,
        Experience = 0,
        Level = 1,
        DailyClaimed = DateTimeOffset.Now,
        DailyStreak = 0,
        LastSeen = DateTimeOffset.Now,
        User = new(),
        CurrentLevelExperience = 0,
        NextLevelExperience = 200,
        TotalVoiceTime = TimeSpan.Zero
    };

    public LevelServiceUnitTest() => _levelService = new(_mockdb.Object, _mockLogger.Object, _mockBot.Object, _mockMediator.Object);

    [Fact]
    public async Task LevelService_AddExperienceAsync_Returns_100()
    {
        // Arrange
        var configDbSet = new List<GuildConfig> { _config }
            .AsQueryable().BuildMockDbSet();
        var levelDbSet = new List<LevelRequirement> { new() { Level = 2, Experience = 400 } }
            .AsQueryable().BuildMockDbSet();
        var userDbSet = new List<GuildUser> { _user }
            .AsQueryable().BuildMockDbSet();
        
        _mockdb.Setup(e => e.GuildConfigs).Returns(configDbSet.Object);
        _mockdb.Setup(e => e.LevelRequirements).Returns(levelDbSet.Object);
        _mockdb.Setup(e => e.Users).Returns(userDbSet.Object);
        
        const int experience = 100;
        const int expected = 100;
        
        // Act
        var actual = await _levelService.AddExperienceAsync(member, experience);
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task LevelService_AddExperienceAsync_Returns_200_And_Level_WithRole()
    {
        // Arrange
        _user.Experience = 300;
        
        var configDbSet = new List<GuildConfig> { _config }
            .AsQueryable().BuildMockDbSet();
        var levelDbSet = new List<LevelRequirement> { new() { Level = 2, Experience = 400 } }
            .AsQueryable().BuildMockDbSet();
        var userDbSet = new List<GuildUser> { _user }
            .AsQueryable().BuildMockDbSet();
        
        _mockdb.Setup(e => e.GuildConfigs).Returns(configDbSet.Object);
        _mockdb.Setup(e => e.LevelRequirements).Returns(levelDbSet.Object);
        _mockdb.Setup(e => e.Users).Returns(userDbSet.Object);
        
        const int experience = 200;
        const int expected = 200;
        
        // Act
        var actual = await _levelService.AddExperienceAsync(member, experience);
        
        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(2, _user.Level);
        Assert.Equal(member.RoleIds, new List<ulong> { 1, 2, 3, 4 });
    }
}