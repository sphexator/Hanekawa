using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Infrastructure.Extensions
{
    public static class DbExtensions
    {
        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceProvider serviceProvider,
            ulong id, TimeSpan? timeSpan = null) where TEntity : class, new()
            => await GetOrReceiveCacheAsync<TEntity>(serviceProvider.CreateScope(), id, timeSpan);

        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceScope serviceScope,
            ulong key, TimeSpan? timeSpan = null) where TEntity : class, new()
        {
            var cache = serviceScope.ServiceProvider.GetRequiredService<ICacheContext>();
            var result = timeSpan.HasValue
                ? await cache.GetAsync<TEntity>($"{nameof(TEntity)}-{key}", timeSpan.Value)
                : await cache.GetAsync<TEntity>($"{nameof(TEntity)}-{key}");
            if (result is not null) return result;
            
            await using var db = serviceScope.ServiceProvider.GetRequiredService<DbService>();
            result = await GetOrCreateEntityAsync<TEntity>(db, key);

            if (timeSpan.HasValue)
                await cache.AddAsync($"{nameof(TEntity)}-{key}", result, timeSpan.Value);
            else
                await cache.AddAsync($"{nameof(TEntity)}-{key}", result);

            return result;
        }

        public static async ValueTask<TEntity> GetOrCreateEntityAsync<TEntity>(this IDbContext context, 
            params ulong[] args) where TEntity : class, new()
        {
            if (context is not DbService dbService) 
                throw new ValidationException("This IDbContext isn't inherited by DbService");
            
            var config = await dbService.FindAsync<TEntity>(args);
            if (config is not null) return config;
            
            config = (TEntity)Activator.CreateInstance(typeof(TEntity), new { args })!;
            await dbService.AddAsync(config);
            await context.SaveChangesAsync();
            return (await dbService.FindAsync<TEntity>(args))!;
        }
    }
}