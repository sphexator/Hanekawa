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

        if (user is null)
        {
            user = new GuildUser
            {
                GuildId = guildId,
                Id = userId,
                User = new User
                {
                    Id = userId,
                }
            };
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return user;
    }
}