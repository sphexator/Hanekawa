using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserJoinedHandler(IModuleService moduleService) : INotificationHandler<UserJoin>
{
    public async Task HandleAsync(UserJoin notification, CancellationToken cancellationToken)
    {
        if (!await moduleService.IsEnabledAsync(notification.GuildId, ModuleName.Logging, cancellationToken))
            return;
    }
}
