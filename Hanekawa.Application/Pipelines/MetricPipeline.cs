using System.Diagnostics;
using Hanekawa.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Pipelines;

public class MetricPipeline<TRequest, TResult> : IPipelineBehavior<IMetric, TResult> where TRequest : notnull
{
    private readonly ILogger<MetricPipeline<TRequest, TResult>> _logger;
    private readonly Metrics _metrics;

    public MetricPipeline(ILogger<MetricPipeline<TRequest, TResult>> logger, Metrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<TResult> Handle(IMetric request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var type = request.GetType();
        _metrics.IncrementCounter(type.Name);
        using var _ = _metrics.MeasureDuration(type.Name);
        var start = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        _logger.LogInformation("Request {Request} executed in {Elapsed}ms",  nameof(request), elapsedTime);
        return response;
    }
}