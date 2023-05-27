using Hanekawa.Application.Commands.Settings;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Microsoft.Extensions.DependencyInjection;
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
        fontCollection.Add("Hanekawa/Data/Fonts/ARIAL.TTF");
        fontCollection.Add("Hanekawa/Data/Fonts/TIMES.TTF");
        
        serviceCollection.AddSingleton(fontCollection);
        
        return serviceCollection;
    }
}