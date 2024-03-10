﻿using Hanekawa.Application.Handlers.Commands.Settings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;
using Prometheus.Client.DependencyInjection;
using SixLabors.Fonts;

namespace Hanekawa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IGreetService, GreetService>();
        serviceCollection.AddScoped<ILevelService, LevelService>();
        serviceCollection.AddScoped<ILevelCommandService, LevelCommandService>();
        serviceCollection.AddScoped<ILogService, LogSettingService>();

        var fontCollection = new FontCollection();
        fontCollection.Add(@"Data/Fonts/ARIAL.TTF");
        fontCollection.Add(@"Data/Fonts/TIMES.TTF");
        
        serviceCollection.AddSingleton(fontCollection);

        serviceCollection.AddMetricFactory(new CollectorRegistry());
        serviceCollection.AddSingleton<IMetrics, Metrics>();
        
        return serviceCollection;
    }
}