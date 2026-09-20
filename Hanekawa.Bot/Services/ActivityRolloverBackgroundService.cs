using Hanekawa.Application.Interfaces.Services;

namespace Hanekawa.Bot.Services;

/// <summary>
/// Periodically checks for activity week rollovers: hands out the previous week's
/// role reward and posts the configured weekly announcement.
/// </summary>
public class ActivityRolloverBackgroundService(IServiceProvider services,
    ILogger<ActivityRolloverBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var activityService = scope.ServiceProvider.GetRequiredService<IActivityService>();
                await activityService.ProcessWeekRolloversAsync(stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Failed to process activity week rollovers");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
