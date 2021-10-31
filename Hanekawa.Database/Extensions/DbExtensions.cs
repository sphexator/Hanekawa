using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
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