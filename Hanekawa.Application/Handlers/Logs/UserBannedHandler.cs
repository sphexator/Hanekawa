using MediatR;

namespace Hanekawa.Application.Handlers.Logs;

public class UserBannedHandler : IRequestHandler<Contracts.Discord.UserBanned>
{
    public Task Handle(Contracts.Discord.UserBanned request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}