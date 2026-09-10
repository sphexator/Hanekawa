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

    [Fact]
    public async Task HandleAsync_UsesConfiguredExperienceBounds_WhenModuleEnabled()
    {
        const int lower = 10;
        const int upper = 20;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["expLower"] = lower.ToString(),
                ["expUpper"] = upper.ToString()
            })
            .Build();
        var captured = new List<int>();
        var levelService = new Mock<ILevelService>();
        levelService.Setup(x => x.AddExperienceAsync(
                It.IsAny<Hanekawa.Entities.Discord.DiscordMember>(), It.IsAny<int>()))
            .Callback<Hanekawa.Entities.Discord.DiscordMember, int>((_, experience) => captured.Add(experience))
            .ReturnsAsync((Hanekawa.Entities.Discord.DiscordMember _, int experience) => experience);
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsEnabledAsync(1, ModuleName.Level, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = new MessageReceivedExperienceHandler(levelService.Object, configuration, moduleService.Object);

        for (var i = 0; i < 40; i++)
        {
            await sut.HandleAsync(CreateNotification(), CancellationToken.None);
        }

        Assert.Equal(40, captured.Count);
        Assert.All(captured, experience => Assert.InRange(experience, lower, upper - 1));
    }
}
