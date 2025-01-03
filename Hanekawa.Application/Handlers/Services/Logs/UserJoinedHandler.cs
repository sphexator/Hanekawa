using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Application.Contracts.Discord.Services;
using MediatR;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserJoinedHandler : INotificationHandler<UserJoin>
{
    public Task Handle(UserJoin request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}