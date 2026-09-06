using Hanekawa.Decorator;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Application.Services;

public interface IEventPublisher
{
    ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : notnull;
}

public sealed class EventPublisher(IServiceScopeFactory scopeFactory) : IEventPublisher
{
    public async ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : notnull
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
