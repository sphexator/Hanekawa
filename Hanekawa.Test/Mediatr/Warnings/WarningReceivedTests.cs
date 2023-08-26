using Hanekawa.Application.Handlers.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Moq;

namespace Hanekawa.Test.Mediatr.Warnings;

public class WarningReceivedTests
{
    private WarningReceivedHandler Mediatr { get; set; } = null!;
    private readonly Mock<IDbContext> _dbContext = new();

    [Fact]
    public async Task WarningReceived_Should_Add_Warning()
    {
        // Arrange
        Mediatr = new WarningReceivedHandler(_dbContext.Object);
        var received =
            new WarningReceived(new DiscordMember { Guild = new Guild { Id = ulong.MinValue }, Username = "", },
                "", 1);
        // Act
        var result = await Mediatr.Handle(received, CancellationToken.None);
        // Assert
    }
    
    [Fact]
    public void WarningReceived_Should_Ban_user()
    {
        
    }
}