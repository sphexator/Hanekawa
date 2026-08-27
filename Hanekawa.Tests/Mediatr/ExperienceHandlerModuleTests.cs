using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities;
using Hanekawa.Tests.Common;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class ExperienceHandlerModuleTests
{
    private static MessageReceived CreateNotification()
        => new(1, 2, TestUsers.TestMember, 3, "hello", DateTimeOffset.UtcNow);

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["expLower"] = "1",
                ["expUpper"] = "5"
            })
            .Build();

    [Fact]
    public async Task HandleAsync_SkipsExperience_WhenLevelModuleDisabled()
    {
        var levelService = new Mock<ILevelService>();
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var sut = new MessageReceivedExperienceHandler(levelService.Object, CreateConfiguration(),
            moduleService.Object);

        await sut.HandleAsync(CreateNotification(), CancellationToken.None);

        levelService.Verify(x => x.AddExperienceAsync(It.IsAny<Hanekawa.Entities.Discord.DiscordMember>(),
            It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AddsExperience_WhenLevelModuleEnabled()
    {
        var levelService = new Mock<ILevelService>();
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = new MessageReceivedExperienceHandler(levelService.Object, CreateConfiguration(),
            moduleService.Object);

        await sut.HandleAsync(CreateNotification(), CancellationToken.None);

        levelService.Verify(x => x.AddExperienceAsync(It.IsAny<Hanekawa.Entities.Discord.DiscordMember>(),
            It.IsAny<int>()), Times.Once);
    }
}
