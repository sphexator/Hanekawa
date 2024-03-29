using System.Linq.Expressions;
using Hanekawa.Entities.Discord;
using Hanekawa.Entities.Users;
using Hanekawa.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Hanekawa.Application.Extensions;

public static class DbExtensions
{
    /// <summary>
    ///  Get or create an entity in the database.
    /// </summary>
    /// <param name="dbSet"></param>
    /// <param name="member"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<GuildUser> GetOrCreateAsync(this DbSet<GuildUser> dbSet, DiscordMember member,
        CancellationToken cancellationToken = default)
    {
        var user = await dbSet.Include(e => e.User)
            .FirstOrDefaultAsync(x => x.GuildId == member.Guild.Id && x.UserId == member.Id,
                cancellationToken);
        if (user is not null) return user;
        
        user = new()
        {
            GuildId = member.Guild.Id,
            UserId = member.Id,
            User = new()
            {
                Id = member.Id,
                PremiumExpiration = null
            }
        };
        await dbSet.AddAsync(user, cancellationToken);
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="predicate"></param>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <returns></returns>
    public static async ValueTask<TEntity> GetOrCreateAsync<TEntity, TProperty>(
        this IIncludableQueryable<TEntity, TProperty> queryable, 
        Expression<Func<TEntity, bool>> predicate, 
        TEntity entity, 
        CancellationToken cancellationToken = default
        ) where TEntity : IMemberEntity
    {
        var user = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        return user ?? entity;
    }
}