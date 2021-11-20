using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace Hanekawa.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContextPool<DbService>(x =>
        {
            x.UseNpgsql(cfg["connectionString"]);
            x.EnableDetailedErrors();
            x.EnableSensitiveDataLogging(false);
        });
        services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(new RedisConfiguration
        {
            AbortOnConnectFail = true,
            KeyPrefix = "hanekawaBot_",
            Hosts = new RedisHost[]
            {
                new (){Host = cfg["redisIp"], Port = 6379}
            },
            AllowAdmin = true,
            ConnectTimeout = 3000,
            Database = 0,
            Ssl = true,
            Password = cfg["redisPassword"],
            ServerEnumerationStrategy = new ServerEnumerationStrategy()
            {
                Mode = ServerEnumerationStrategy.ModeOptions.All,
                TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
            },
            MaxValueLength = 512000000,
            PoolSize = 5
        });

        return services;
    }
    
    /// <summary>
    /// Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfiguration">The redis configration.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
    public static IServiceCollection AddStackExchangeRedisExtensions<T>(this IServiceCollection services, RedisConfiguration redisConfiguration)
        where T : class, ISerializer, new()
    {
        services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
        services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
        services.AddSingleton<ISerializer, T>();

        services.AddSingleton((provider) => provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());
        services.AddSingleton(redisConfiguration);

        return services;
    }
}