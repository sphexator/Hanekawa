using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Decorator;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserJoinedHandler : INotificationHandler<UserJoin>
{
    public Task HandleAsync(UserJoin notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
