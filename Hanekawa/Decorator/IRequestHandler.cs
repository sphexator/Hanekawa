namespace Hanekawa.Decorator;

public interface IRequestHandler<in TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IPipelineHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
{
}

public interface IRequest
{
}

public interface IRequest<out TResponse>
{
}

public interface INotification
{
}

public interface INotificationHandler<in TNotification>
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}