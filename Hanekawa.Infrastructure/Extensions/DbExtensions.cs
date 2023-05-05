using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Infrastructure.Extensions
{
    public static class DbExtensions
    {
        /*
        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceProvider serviceProvider,
            ulong id, TimeSpan? timeSpan = null) where TEntity : class, new()
            => await GetOrReceiveCacheAsync<TEntity>(serviceProvider.CreateScope(), id);

        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceScope serviceScope,
            ulong key, TimeSpan? timeSpan = null) where TEntity : class, new()
        {
            var cache = serviceScope.ServiceProvider.GetRequiredService<IRedisCacheClient>();
            var result = timeSpan.HasValue
                ? await cache.Db0.GetAsync<TEntity>($"{nameof(TEntity)}-{key}", timeSpan.Value)
                : await cache.Db0.GetAsync<TEntity>($"{nameof(TEntity)}-{key}");
            if (result != null) return result;
            
            await using var db = serviceScope.ServiceProvider.GetRequiredService<DbService>();
            result = await GetOrCreateEntityAsync<TEntity>(db, key);

            if (timeSpan.HasValue)
                await cache.Db0.AddAsync($"{nameof(TEntity)}-{key}", result, timeSpan.Value);
            else
                await cache.Db0.AddAsync($"{nameof(TEntity)}-{key}", result);

            return result;
        }

        public static async ValueTask<TEntity?> GetOrCreateEntityAsync<TEntity>(this DbService context, 
            params ulong[] args) where TEntity : class, new()
        {
            var config = await context.FindAsync<TEntity>(args);
            if (config != null) return config;
            
            config = (TEntity)Activator.CreateInstance(typeof(TEntity), new { args })!;
            try
            {
                await context.AddAsync(config);
                await context.SaveChangesAsync();
                return await context.FindAsync<TEntity>(args);
            }
            catch
            {
                return config;
            }
        }
        */
    }
}