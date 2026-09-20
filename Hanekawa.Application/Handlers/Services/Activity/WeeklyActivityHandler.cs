using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;

namespace Hanekawa.Application.Handlers.Services.Activity;

public class WeeklyActivityHandler(IModuleService moduleService, IActivityService activityService)
    : INotificationHandler<MessageRateLimitPassed>
{
    public async Task HandleAsync(MessageRateLimitPassed notification, CancellationToken cancellationToken)
    {
        if (!await moduleService.IsEnabledAsync(notification.GuildId, ModuleName.Activity, cancellationToken))
            return;

        await activityService.TrackMessageAsync(notification.GuildId, notification.Member.Id,
            notification.CreatedAt, cancellationToken);
        await activityService.SyncCurrentWeekRolesAsync(notification.GuildId, cancellationToken);
    }
}
