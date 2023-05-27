using Hanekawa.Application.Interfaces;
using MediatR;

namespace Hanekawa.Application.Handlers.Warnings;

public record WarningList(ulong GuildId, ulong? UserId) : IRequest<List<string>>;

public class WarningListHandler : IRequestHandler<WarningList, List<string>>
{
    public Task<List<string>> Handle(WarningList request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}