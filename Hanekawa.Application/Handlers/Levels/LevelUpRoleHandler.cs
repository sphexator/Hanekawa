using Hanekawa.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Levels;

public class LevelUpRoleHandler : IRequestHandler<Contracts.LevelUp>
{
    private readonly ILogger<LevelUpRoleHandler> _logger;
    private readonly ILevelService _levelService;

    public LevelUpRoleHandler(ILogger<LevelUpRoleHandler> logger, ILevelService levelService)
    {
        _logger = logger;
        _levelService = levelService;
    }

    public Task Handle(Contracts.LevelUp request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}