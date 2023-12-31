using Hanekawa.Application.Contracts;
using Hanekawa.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Services.Levels;

public class LevelUpRoleHandler : IRequestHandler<LevelUp>
{
    private readonly ILogger<LevelUpRoleHandler> _logger;
    private readonly ILevelService _levelService;

    public LevelUpRoleHandler(ILogger<LevelUpRoleHandler> logger, ILevelService levelService)
    {
        _logger = logger;
        _levelService = levelService;
    }

    /// <summary>
    /// Handles the level up event.
    /// </summary>
    /// <param name="request">User information containing the new level of user</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(LevelUp request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("handing out roles for user {UserId} in guild {GuildId} for level {Level}", 
            request.Member.Id, request.Member.Guild.Id, request.Level);
        await _levelService.AdjustRolesAsync(request.Member, request.Level, request.GuildConfig);
    }
}