using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Extensions;

public static class DbExtensions
{
    public static async ValueTask<GuildUser> GetOrCreateUserAsync(this IDbContext dbContext,
        ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(e => e.User)
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == userId, cancellationToken);

        if (user is not null) return user;

        // User is global (PK = Discord snowflake). Reuse it when this person
        // already exists in another guild so we don't insert a duplicate row.
        var existingGlobalUser = await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.User)
            .FirstOrDefaultAsync(cancellationToken);

        user = new GuildUser
        {
            GuildId = guildId,
            Id = userId,
            User = existingGlobalUser ?? new User { Id = userId },
        };
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}