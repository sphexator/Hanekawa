using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Hanekawa.Database.Extensions
{
    public static class DbExtensions
    {
        internal static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter) 
            => modelBuilder.UseValueConverterForType(typeof(T), converter);

        private static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == type);
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(converter);
                }
            }

            return modelBuilder;
        }
        
        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceProvider serviceProvider,
            Snowflake id, TimeSpan? timeSpan = null) where TEntity : class, new()
            => await GetOrReceiveCacheAsync<TEntity>(serviceProvider.CreateScope(), id);

        public static async ValueTask<TEntity> GetOrReceiveCacheAsync<TEntity>(this IServiceScope serviceScope,
            Snowflake key, TimeSpan? timeSpan = null) where TEntity : class, new()
        {
            var cache = serviceScope.ServiceProvider.GetRequiredService<IRedisCacheClient>();
            var result = timeSpan.HasValue
                ? await cache.Db0.GetAsync<TEntity>($"{nameof(TEntity)}-{key.RawValue}", timeSpan.Value)
                : await cache.Db0.GetAsync<TEntity>($"{nameof(TEntity)}-{key.RawValue}");
            if (result != null) return result;
            
            await using var db = serviceScope.ServiceProvider.GetRequiredService<DbService>();
            result = await GetOrCreateEntityAsync<TEntity>(db, key);

            if (timeSpan.HasValue)
                await cache.Db0.AddAsync($"{nameof(TEntity)}-{key.RawValue}", result, timeSpan.Value);
            else
                await cache.Db0.AddAsync($"{nameof(TEntity)}-{key.RawValue}", result);

            return result;
        }

        public static async ValueTask<TEntity> GetOrCreateEntityAsync<TEntity>(this DbService context, 
            params Snowflake[] args) where TEntity : class, new()
        {
            var config = await context.FindAsync<TEntity>(args);
            if (config != null) return config;
            
            config = (TEntity)Activator.CreateInstance(typeof(TEntity), new { args });
            try
            {
                if (config != null) await context.AddAsync(config);
                await context.SaveChangesAsync();
                return await context.FindAsync<TEntity>(args);
            }
            catch
            {
                return config;
            }
        }
        
        public static async ValueTask<TEntity> CreateIncrementEntityAsync<TEntity>(this DbService context,
            Snowflake primaryId, Snowflake secondaryId) where TEntity : class, new()
        {
            var counter = await context.Suggestions.CountAsync(x => x.GuildId == primaryId);
            var nr = counter == 0 ? 1 : counter + 1;
        
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity), new { nr, primaryId, secondaryId });
            try
            {
                await context.AddAsync(entity);
                await context.SaveChangesAsync();
                return await context.FindAsync<TEntity>(nr, primaryId);
            }
            catch
            {
                return entity;
            }
        }
    }
}