using System;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Infrastructure.Extensions
{
    public static partial class DbExtensions
    {
        public static async ValueTask<TEntity> GetOrCreateEntityAsync<TEntity>(this IDbContext context, 
            params ulong[] args) where TEntity : class, new()
        {
            var config = await context.FindAsync<TEntity>(args).ConfigureAwait(false);
            if (config != null) return config;
            
            config = (TEntity)Activator.CreateInstance(typeof(TEntity), new { args });
            try
            {
                if (config != null) await context.AddAsync(config).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.FindAsync<TEntity>(args).ConfigureAwait(false);
            }
            catch
            {
                return config;
            }
        }
        
        public static async ValueTask<TEntity> CreateIncrementEntityAsync<TEntity>(this IDbContext context,
            ulong primaryId, ulong secondaryId) where TEntity : class, new()
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