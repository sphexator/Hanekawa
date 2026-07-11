using Hanekawa.Application.Pipelines;
using Hanekawa.Decorator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Extensions;

public static class IRequestHandlerRegistrationExtensions
{
    public static IServiceCollection AddDecoratedRequestHandler<TRequest, THandler>(
        this IServiceCollection services)
        where TRequest : notnull
        where THandler : class, IRequestHandler<TRequest>
    {
        services.AddScoped<THandler>();

        services.AddScoped<IRequestHandler<TRequest>>(provider =>
        {
            IRequestHandler<TRequest> handler = provider.GetRequiredService<THandler>();

            handler = new MetricPipeline<TRequest>(handler,
                provider.GetRequiredService<ILogger<MetricPipeline<TRequest>>>(),
                provider.GetRequiredService<Metrics>());

            return handler;
        });

        return services;
    }

    public static IServiceCollection AddDecoratedRequestHandler<TRequest, THandler>(
        this IServiceCollection services,
        params Type[] pipelineTypes)
        where TRequest : notnull
        where THandler : class, IRequestHandler<TRequest>
    {
        services.AddScoped<THandler>();

        foreach (var pipelineType in pipelineTypes)
        {
            services.AddScoped(pipelineType);
        }

        services.AddScoped<IRequestHandler<TRequest>>(provider =>
        {
            IRequestHandler<TRequest> handler = provider.GetRequiredService<THandler>();

            handler = new MetricPipeline<TRequest>(handler,
                provider.GetRequiredService<ILogger<MetricPipeline<TRequest>>>(),
                provider.GetRequiredService<Metrics>());

            foreach (var pipelineType in pipelineTypes)
            {
                handler = (IRequestHandler<TRequest>)ActivatorUtilities.CreateInstance(
                    provider, pipelineType, handler);
            }

            return handler;
        });

        return services;
    }

    public static IServiceCollection AddDecoratedRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services)
        where TRequest : notnull
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddScoped<THandler>();

        services.AddScoped<IRequestHandler<TRequest, TResponse>>(provider =>
        {
            IRequestHandler<TRequest, TResponse> handler = provider.GetRequiredService<THandler>();

            handler = new MetricPipeline<TRequest, TResponse>(handler,
                provider.GetRequiredService<ILogger<MetricPipeline<TRequest, TResponse>>>(),
                provider.GetRequiredService<Metrics>());

            return handler;
        });

        return services;
    }

    public static IServiceCollection AddDecoratedRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services,
        params Type[] pipelineTypes)
        where TRequest : notnull
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddScoped<THandler>();

        foreach (var pipelineType in pipelineTypes)
        {
            services.AddScoped(pipelineType);
        }

        services.AddScoped<IRequestHandler<TRequest, TResponse>>(provider =>
        {
            IRequestHandler<TRequest, TResponse> handler = provider.GetRequiredService<THandler>();

            handler = new MetricPipeline<TRequest, TResponse>(handler,
                provider.GetRequiredService<ILogger<MetricPipeline<TRequest, TResponse>>>(),
                provider.GetRequiredService<Metrics>());

            foreach (var pipelineType in pipelineTypes)
            {
                handler = (IRequestHandler<TRequest, TResponse>)ActivatorUtilities.CreateInstance(
                    provider, pipelineType, handler);
            }

            return handler;
        });

        return services;
    }
}