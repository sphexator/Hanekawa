using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Decorator;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserLeftHandler : INotificationHandler<UserLeave>
{
    public Task HandleAsync(UserLeave notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}