using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Pipelines;

public class MetricPipeline<TRequest, TResult> : IPipelineBehavior<TRequest, TResult> where TRequest : notnull
{
    private readonly ILogger<MetricPipeline<TRequest, TResult>> _logger;

    public MetricPipeline(ILogger<MetricPipeline<TRequest, TResult>> logger) => _logger = logger;

    public async Task<TResult> Handle(TRequest request, 
        RequestHandlerDelegate<TResult> next, 
        CancellationToken cancellationToken)
    {
        var start = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        
        _logger.LogInformation("Request {Request} executed in {Elapsed}ms",  nameof(request), elapsedTime);
        return response;
    }
}