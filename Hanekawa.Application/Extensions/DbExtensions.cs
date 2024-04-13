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
    /// <returns></returns>
    public static async ValueTask<T1> GetOrCreateAsync<T1, T2>(
        this IIncludableQueryable<T1, T2> queryable, 
        Expression<Func<T1, bool>> predicate, 
        T1 entity, 
        CancellationToken cancellationToken = default
        ) where T1 : IMemberEntity
    {
        var user = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        return user ?? entity;
    }
}