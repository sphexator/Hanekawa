using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Configs;

namespace Hanekawa.Application.Handlers.Services.Internal;

public interface IBotService
{
    ValueTask LeftGuildAsync(ulong guildId);
    ValueTask JoinedGuildAsync(ulong guildId);
}

public class BotService : IBotService
{
    private readonly IDbContext _db;

    public BotService(IDbContext db)
    {
        _db = db;
    }

    public async ValueTask LeftGuildAsync(ulong guildId)
    {
        var guild = await _db.GuildConfigs.FindAsync(guildId);
        if (guild is not null)
        {
            return;
        }

        await _db.GuildConfigs.AddAsync(new GuildConfig
        {
            GuildId = guildId

        });
        await _db.SaveChangesAsync();
    }

    public async ValueTask JoinedGuildAsync(ulong guildId)
    {
        var guild = await _db.GuildConfigs.FindAsync(guildId);
        if (guild is null)
        {
            return;
        }

        _db.GuildConfigs.Remove(guild);
        await _db.SaveChangesAsync();
    }
}