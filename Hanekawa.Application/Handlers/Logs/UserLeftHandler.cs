using Hanekawa.Application.Contracts.Discord;
using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserLeftHandler : IRequestHandler<UserLeave>
{
    public Task Handle(UserLeave request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}