using Hanekawa.Decorator;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Application.Services;

public interface IRequestDispatcher
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull;

    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull;
}

public sealed class RequestDispatcher(IServiceScopeFactory scopeFactory) : IRequestDispatcher
{
    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
