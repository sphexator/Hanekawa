using System.Linq.Expressions;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Users;
using Hanekawa.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Hanekawa.Application.Extensions;

public static class DbExtensions
{
    public static async ValueTask<GuildConfig> GetOrCreateConfigAsync(this IDbContext dbContext, ulong guildId,
        CancellationToken cancellationToken = default)
    {
        var config = await dbContext.GuildConfigs.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
        if (config is not null) return config;

        config = new GuildConfig
        {
            GuildId = guildId
        };
        await dbContext.GuildConfigs.AddAsync(config, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return config;
    }

    public static async ValueTask<GuildConfig> GetOrCreateConfigAsync(this IDbContext dbContext,
        ulong guildId, Type expression,
        CancellationToken cancellationToken = default)
    {
        var config = await dbContext.GuildConfigs.Include(expression.Name)
                .FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);

        if (config is not null) return config;

        config = new GuildConfig
        {
            GuildId = guildId
        };
        await dbContext.GuildConfigs.AddAsync(config, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return config;
    }

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