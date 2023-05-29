using Hanekawa.Application.Contracts.Discord;
using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserUnbannedHandler : IRequestHandler<UserUnbanned, bool>
{
    public Task<bool> Handle(Contracts.Discord.UserUnbanned request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}