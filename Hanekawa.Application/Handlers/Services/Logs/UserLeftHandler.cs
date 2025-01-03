using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Application.Contracts.Discord.Services;
using MediatR;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserLeftHandler : INotificationHandler<UserLeave>
{
    public Task Handle(UserLeave request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}