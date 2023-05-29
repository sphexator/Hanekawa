using Hanekawa.Application.Contracts.Discord;
using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserLeftHandler : IRequestHandler<UserLeave, bool>
{
    public Task<bool> Handle(UserLeave request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}