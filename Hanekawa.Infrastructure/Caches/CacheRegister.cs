using System;
using System.Linq;
using System.Reflection;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Hanekawa.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Infrastructure.Caches;

internal static class CacheRegister
{
    public static void RegisterCacheProviders(this IServiceCollection services)
    {
        var interfaces = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                           type.GetInterfaces()
                               .Any(i => i == typeof(ICached)))
            .ToArray().AsSpan();
        for (var i = 0; i < interfaces.Length; i++)
        {
            var x = interfaces[i];
            var cacheService = typeof(CacheService<>).MakeGenericType(x);
            services.AddScoped(typeof(ICacheContext<>).MakeGenericType(x), provider =>
            {
                var cache = provider.GetRequiredService<IDistributedCache>();
                var logger = provider.GetRequiredService<ILogger<ICacheContext<ICached>>>();
                return Activator.CreateInstance(cacheService, cache, logger)!;
            });
        }
    }
}