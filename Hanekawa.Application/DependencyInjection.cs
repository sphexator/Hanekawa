using System.Reflection;
using Hanekawa.Application.Handlers.Commands.Account;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Handlers.Commands.Club;
using Hanekawa.Application.Handlers.Commands.Settings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Application.Services.Images;
using Hanekawa.Entities.Settings.Images;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;
using Prometheus.Client.DependencyInjection;
using SixLabors.Fonts;

namespace Hanekawa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ImageSettings>(configuration.GetSection("ImageSettings"));
        serviceCollection.AddScoped<ILevelService, LevelService>();
        serviceCollection.AddScoped<IDropService, DropService>();
        serviceCollection.AddScoped<IImageService, ImageService>();
        serviceCollection.AddScoped<IConfigService, ConfigService>();

        serviceCollection.AddScoped<IAdministrationCommandService, AdministrationCommandService>();
        serviceCollection.AddScoped<ILogService, LogSettingService>();
        serviceCollection.AddScoped<IGreetService, GreetService>();
        serviceCollection.AddScoped<IClubCommandService, ClubCommandService>();
        serviceCollection.AddScoped<ILevelCommandService, LevelCommandService>();
        serviceCollection.AddScoped<AccountCommandService>();
        //serviceCollection.AddScoped<IWarningCommandService>();

        var fontCollection = new FontCollection();
        fontCollection.Add(@"Data/Fonts/ARIAL.TTF");
        fontCollection.Add(@"Data/Fonts/TIMES.TTF");
        fontCollection.AddSystemFonts();
        serviceCollection.AddSingleton(fontCollection);
        serviceCollection.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblies(Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly());
        });
        serviceCollection.AddMetricFactory(new CollectorRegistry());
        serviceCollection.AddSingleton<IMetrics, Metrics>();

        return serviceCollection;
    }
}