using MediatR;

namespace Hanekawa.Application.Handlers.Warnings;

public record WarningClear(ulong GuildId, ulong UserId, ulong ModeratorId, string? Reason, bool All = false) : IRequest;

public class WarningClearHandler : IRequestHandler<WarningClear>
{
    public Task Handle(WarningClear request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}