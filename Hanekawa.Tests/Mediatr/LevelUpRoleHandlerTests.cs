using Hanekawa.Application.Contracts;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hanekawa.Tests.Mediatr;

public class LevelUpRoleHandlerTests
{
    [Fact]
    public async Task HandleAsync_AdjustsRoles_WithMemberLevelAndGuildConfigFromRequest()
    {
        var levelService = new Mock<ILevelService>();
        var member = new DiscordMember
        {
            Id = 1,
            Guild = new Guild { GuildId = 1 },
            Username = "Bob"
        };
        var config = new GuildConfig { GuildId = 1 };
        var sut = new LevelUpRoleHandler(Mock.Of<ILogger<LevelUpRoleHandler>>(), levelService.Object);

        await sut.HandleAsync(new LevelUp(member, member.RoleIds, 5, config), CancellationToken.None);

        levelService.Verify(x => x.AdjustRolesAsync(member, 5, config), Times.Once);
    }
}
