using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Extensions;

public static class DbExtensions
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static async ValueTask<GuildUser> GetOrCreateUserAsync(this IDbContext dbContext,
        ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(e => e.User)
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == userId, cancellationToken);

        if (user is not null) return user;

        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            user = await dbContext.Users
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
        catch (DbUpdateException)
        {
            // Another instance may have inserted the row; detach our failed insert so later
            // SaveChanges on this context does not retry it or leave callers with a detached entity.
            DetachIfTracked(dbContext, user);

            var existingUser = await dbContext.Users
                .Include(e => e.User)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == userId, cancellationToken);

            if (existingUser is not null)
            {
                return existingUser;
            }

            throw;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static void DetachIfTracked(IDbContext dbContext, GuildUser user)
    {
        try
        {
            var entry = dbContext.Users.Entry(user);
            // AddAsync tracks a brand-new global User as Added. Detaching only the guild
            // user leaves that insert pending, so the next SaveChanges retries it and
            // loses the experience update on the row that won the race.
            if (entry.Reference(static x => x.User).TargetEntry is { State: EntityState.Added } globalUser)
                globalUser.State = EntityState.Detached;

            entry.State = EntityState.Detached;
        }
        catch (Exception ex) when (ex is NullReferenceException or InvalidOperationException)
        {
            // Unit-test DbSet mocks may not implement Entry; production DbContext does.
        }
    }
}