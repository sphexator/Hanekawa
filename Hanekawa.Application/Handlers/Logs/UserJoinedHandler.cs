using Hanekawa.Application.Contracts.Discord;
using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserJoinedHandler : IRequestHandler<UserJoin>
{
    public Task Handle(UserJoin request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}