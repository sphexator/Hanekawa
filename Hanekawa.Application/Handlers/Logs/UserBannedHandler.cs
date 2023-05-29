using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserBannedHandler : IRequestHandler<Contracts.Discord.UserBanned, bool>
{
    public Task<bool> Handle(Contracts.Discord.UserBanned request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}