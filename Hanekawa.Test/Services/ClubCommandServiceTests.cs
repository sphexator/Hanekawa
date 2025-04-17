using Hanekawa.Application.Handlers.Commands.Club;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Club;
using Moq;
using Moq.EntityFrameworkCore;

namespace Hanekawa.Test.Services;

public class ClubCommandServiceTests
{
    private readonly Mock<IDbContext> _mockDbContext;
    private readonly ClubCommandService _clubCommandService;
    private readonly ulong _guildId = 123456789;
    private readonly ulong _userId = 987654321;

    public ClubCommandServiceTests()
    {
        _mockDbContext = new Mock<IDbContext>();
        _clubCommandService = new ClubCommandService(_mockDbContext.Object);
    }

    [Fact]
    public async Task Create_Should_CreateNewClub_When_ValidInput()
    {
        // Arrange
        var clubs = new List<Club>();
        var clubMembers = new List<ClubMember>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);
        _mockDbContext.Setup(db => db.ClubMembers).ReturnsDbSet(clubMembers);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _clubCommandService.Create(_guildId, "TestClub", "Test Description", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'TestClub' has been created successfully!", result.Value.Content);
        Assert.Single(clubs);
        Assert.Single(clubMembers);
        Assert.Equal("TestClub", clubs[0].Name);
        Assert.Equal(_userId, clubs[0].OwnerId);
        Assert.Equal(_userId, clubMembers[0].UserId);
    }

    [Fact]
    public async Task Create_Should_ReturnError_When_ClubNameAlreadyExists()
    {
        // Arrange
        var existingClub = new Club { GuildId = _guildId, Name = "TestClub", OwnerId = 111111 };
        var clubs = new List<Club> { existingClub };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Create(_guildId, "TestClub", "Test Description", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("A club with the name 'TestClub' already exists", result.Value.Content);
        Assert.Single(clubs); // No new club added
    }

    [Fact]
    public async Task Create_Should_ReturnError_When_UserAlreadyOwnsClub()
    {
        // Arrange
        var existingClub = new Club { GuildId = _guildId, Name = "OtherClub", OwnerId = _userId };
        var clubs = new List<Club> { existingClub };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Create(_guildId, "TestClub", "Test Description", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You already own a club. You can only own one club at a time.", result.Value.Content);
        Assert.Single(clubs); // No new club added
    }

    [Fact]
    public async Task Delete_Should_DeleteClub_When_UserIsOwner()
    {
        // Arrange
        var existingClub = new Club { GuildId = _guildId, Name = "TestClub", OwnerId = _userId };
        var clubs = new List<Club> { existingClub };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _clubCommandService.Delete(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'TestClub' has been deleted successfully", result.Value.Content);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_ReturnError_When_ClubDoesNotExist()
    {
        // Arrange
        var clubs = new List<Club>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Delete(_guildId, "NonExistentClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'NonExistentClub' doesn't exist", result.Value.Content);
    }

    [Fact]
    public async Task Delete_Should_ReturnError_When_UserIsNotOwner()
    {
        // Arrange
        var existingClub = new Club { GuildId = _guildId, Name = "TestClub", OwnerId = 111111 }; // Different owner
        var clubs = new List<Club> { existingClub };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Delete(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You don't have permission to delete this club", result.Value.Content);
    }

    [Fact]
    public async Task List_Should_ListAllClubs()
    {
        // Arrange
        var clubs = new List<Club>
        {
            new Club { GuildId = _guildId, Name = "Club1", Description = "First club", Members = new List<ClubMember> { new ClubMember() } },
            new Club { GuildId = _guildId, Name = "Club2", Description = "Second club", Members = new List<ClubMember> { new ClubMember(), new ClubMember() } }
        };

        var queryableClubs = clubs.AsQueryable();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(queryableClubs);

        // Act
        var result = await _clubCommandService.List(_guildId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("**Clubs in this server:**", result.Value.Content);
        Assert.Contains("Club1", result.Value.Content);
        Assert.Contains("Club2", result.Value.Content);
        Assert.Contains("(1 members)", result.Value.Content);
        Assert.Contains("(2 members)", result.Value.Content);
    }

    [Fact]
    public async Task List_Should_ReturnMessage_When_NoClubsExist()
    {
        // Arrange
        var clubs = new List<Club>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.List(_guildId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("No clubs found in this server", result.Value.Content);
    }

    [Fact]
    public async Task Join_Should_AddUserToClub()
    {
        // Arrange
        var club = new Club { GuildId = _guildId, Name = "TestClub", Members = new List<ClubMember>() };
        var clubs = new List<Club> { club };
        var clubMembers = new List<ClubMember>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);
        _mockDbContext.Setup(db => db.ClubMembers).ReturnsDbSet(clubMembers);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _clubCommandService.Join(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You have joined the club 'TestClub'", result.Value.Content);
        Assert.Single(clubMembers);
        Assert.Equal(_userId, clubMembers[0].UserId);
    }

    [Fact]
    public async Task Join_Should_ReturnError_When_ClubDoesNotExist()
    {
        // Arrange
        var clubs = new List<Club>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Join(_guildId, "NonExistentClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'NonExistentClub' doesn't exist", result.Value.Content);
    }

    [Fact]
    public async Task Join_Should_ReturnError_When_UserAlreadyMember()
    {
        // Arrange
        var member = new ClubMember { GuildId = _guildId, ClubName = "TestClub", UserId = _userId };
        var club = new Club { GuildId = _guildId, Name = "TestClub", Members = new List<ClubMember> { member } };
        var clubs = new List<Club> { club };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Join(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You are already a member of this club", result.Value.Content);
    }

    [Fact]
    public async Task Leave_Should_RemoveUserFromClub()
    {
        // Arrange
        var member = new ClubMember { GuildId = _guildId, ClubName = "TestClub", UserId = _userId };
        var club = new Club
        {
            GuildId = _guildId,
            Name = "TestClub",
            OwnerId = 111111, // Different owner
            Members = new List<ClubMember> { member }
        };
        var clubs = new List<Club> { club };
        var clubMembers = new List<ClubMember> { member };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);
        _mockDbContext.Setup(db => db.ClubMembers).ReturnsDbSet(clubMembers);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _clubCommandService.Leave(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You have left the club 'TestClub'", result.Value.Content);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Leave_Should_ReturnError_When_ClubDoesNotExist()
    {
        // Arrange
        var clubs = new List<Club>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Leave(_guildId, "NonExistentClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'NonExistentClub' doesn't exist", result.Value.Content);
    }

    [Fact]
    public async Task Leave_Should_ReturnError_When_UserIsOwner()
    {
        // Arrange
        var club = new Club
        {
            GuildId = _guildId,
            Name = "TestClub",
            OwnerId = _userId,
            Members = new List<ClubMember> { new ClubMember { UserId = _userId } }
        };
        var clubs = new List<Club> { club };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Leave(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("As the owner, you cannot leave your club. You must delete it or transfer ownership first.", result.Value.Content);
    }

    [Fact]
    public async Task Leave_Should_ReturnError_When_UserNotMember()
    {
        // Arrange
        var club = new Club
        {
            GuildId = _guildId,
            Name = "TestClub",
            OwnerId = 111111,
            Members = new List<ClubMember>() // Empty members list
        };
        var clubs = new List<Club> { club };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Leave(_guildId, "TestClub", _userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("You are not a member of this club", result.Value.Content);
    }

    [Fact]
    public async Task Info_Should_ReturnClubDetails()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddDays(-10);
        var joinedAt = DateTimeOffset.UtcNow.AddDays(-5);

        var member = new ClubMember
        {
            GuildId = _guildId,
            ClubName = "TestClub",
            UserId = _userId,
            JoinedAt = joinedAt
        };

        var club = new Club
        {
            GuildId = _guildId,
            Name = "TestClub",
            Description = "Test Description",
            OwnerId = _userId,
            CreatedAt = createdAt,
            Members = new List<ClubMember> { member }
        };

        var clubs = new List<Club> { club };

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Info(_guildId, "TestClub");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("**Club: TestClub**", result.Value.Content);
        Assert.Contains("Description: Test Description", result.Value.Content);
        Assert.Contains($"Owner: <@{_userId}>", result.Value.Content);
        Assert.Contains($"Created: {createdAt:yyyy-MM-dd}", result.Value.Content);
        Assert.Contains("Members: 1", result.Value.Content);
        Assert.Contains("**Member List:**", result.Value.Content);
        Assert.Contains($"<@{_userId}> (joined: {joinedAt:yyyy-MM-dd}) 👑", result.Value.Content);
    }

    [Fact]
    public async Task Info_Should_ReturnError_When_ClubDoesNotExist()
    {
        // Arrange
        var clubs = new List<Club>();

        _mockDbContext.Setup(db => db.Clubs).ReturnsDbSet(clubs);

        // Act
        var result = await _clubCommandService.Info(_guildId, "NonExistentClub");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Club 'NonExistentClub' doesn't exist", result.Value.Content);
    }
}