using Hanekawa.Application.Handlers.Warnings;
using Hanekawa.Application.Interfaces;
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
        Mediatr = new(_dbContext.Object);
        var received =
            new WarningReceived(new() { Guild = new() { Id = ulong.MinValue }, Username = "", },
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