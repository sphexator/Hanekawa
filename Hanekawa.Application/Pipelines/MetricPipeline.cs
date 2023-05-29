using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using IMetric = Hanekawa.Application.Interfaces.IMetric;

namespace Hanekawa.Application.Pipelines;

public class MetricPipeline<TRequest, TResult> : IPipelineBehavior<IMetric, TResult> where TRequest : notnull
{
    private readonly ILogger<MetricPipeline<TRequest, TResult>> _logger;
    private readonly IMetricFactory _metricFactory;

    public MetricPipeline(ILogger<MetricPipeline<TRequest, TResult>> logger, IMetricFactory metricFactory)
    {
        _logger = logger;
        _metricFactory = metricFactory;
    }

    public async Task<TResult> Handle(IMetric request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var type = request.GetType();
        _metricFactory.CreateCounter(type.Name, "Global events", "Request").Inc();
        _metricFactory.CreateCounter(request.GuildId + "-" + type.Name, "Guild specific events",
            $"{request.GuildId}", type.Name, "Request").Inc();
        var start = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        
        _logger.LogInformation("Request {Request} executed in {Elapsed}ms",  nameof(request), elapsedTime);
        return response;
    }
}