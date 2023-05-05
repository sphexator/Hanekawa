using Hanekawa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContextPool<IDbContext, DbService>(x =>
        {
            x.UseNpgsql(cfg["connectionString"]);
            x.EnableDetailedErrors();
            x.EnableSensitiveDataLogging(false);
        });

        return services;
    }
}