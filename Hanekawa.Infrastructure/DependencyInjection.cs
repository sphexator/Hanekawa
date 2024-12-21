using System;
using Hanekawa.Application.Interfaces;
using Hanekawa.Infrastructure.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Hanekawa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddStackExchangeRedisCache(x =>
        {
            x.ConfigurationOptions = new ConfigurationOptions
            {
                ClientName = "Hanekawa",
                AbortOnConnectFail = true,
                Protocol = RedisProtocol.Resp3,
                DefaultDatabase = 1,
                User = "default"
            };
            x.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync(cfg["redis"]
                ?? throw new InvalidOperationException("Redis config is null"));
        });
        services.AddDbContextPool<IDbContext, DbService>(x =>
        {
            x.UseNpgsql(cfg["connectionString"]);
            x.EnableDetailedErrors();
            x.EnableSensitiveDataLogging(false);
            x.UseTriggers(builder =>
            {
                builder.AddTrigger<ModLogBeforeTrigger>();
                builder.AddTrigger<ModLogAfterTrigger>();
            });
        });

        return services;
    }
}