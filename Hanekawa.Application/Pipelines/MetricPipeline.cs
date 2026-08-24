using System.Diagnostics;
using Hanekawa.Application.Interfaces;
using Hanekawa.Decorator;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Pipelines;

public sealed class MetricPipeline<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner,
    ILogger<MetricPipeline<TRequest, TResponse>> logger,
    IMetrics metrics)
    : IRequestHandler<TRequest, TResponse>
{

    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling request {Request}", nameof(request));

        metrics.IncrementCounter(nameof(IRequest));
        using var _ = metrics.MeasureDuration(nameof(IRequest));
        var start = Stopwatch.GetTimestamp();

        var response = await inner.HandleAsync(request, cancellationToken).ConfigureAwait(false);

        var elapsedTime = Stopwatch.GetElapsedTime(start);
        logger.LogInformation("Request {Request} executed in {Elapsed}ms",  nameof(request), elapsedTime);
        return response;
    }
}

public sealed class MetricPipeline<TRequest>(
    IRequestHandler<TRequest> inner,
    ILogger<MetricPipeline<TRequest>> logger,
    IMetrics metrics)
    : IRequestHandler<TRequest>
{
    public async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling request {Request}", nameof(request));

        metrics.IncrementCounter(nameof(IRequest));
        using var _ = metrics.MeasureDuration(nameof(IRequest));
        var start = Stopwatch.GetTimestamp();

        await inner.HandleAsync(request, cancellationToken).ConfigureAwait(false);

        var elapsedTime = Stopwatch.GetElapsedTime(start);
        logger.LogInformation("Request {Request} executed in {Elapsed}ms",  nameof(request), elapsedTime);
    }
}