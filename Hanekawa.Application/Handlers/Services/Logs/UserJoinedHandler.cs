using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Application.Contracts.Discord.Services;
using MediatR;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserJoinedHandler : IRequestHandler<UserJoin, bool>
{
    public Task<bool> Handle(UserJoin request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}